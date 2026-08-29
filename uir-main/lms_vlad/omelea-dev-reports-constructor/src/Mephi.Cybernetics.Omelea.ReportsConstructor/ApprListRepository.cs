using Mephi.Cybernetics.Omelea.ReportsConstructor.Models;
using Mephi.Cybernetics.Race.ReportsDescriptionLanguage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

//

namespace Mephi.Cybernetics.Omelea.ReportsConstructor
{

    public class ApprListRepository : IApprListRepository, IDisposable
    {
        private IExpressionCompiler _expressionCompiler;
        private OmeleaReportsContext _context;
        private ConceptDtoConverter _dtoConceptConverter;

        public ApprListRepository(OmeleaReportsContext context, IExpressionCompiler expressionCompiler)
        {
            _context = context;
            _expressionCompiler = expressionCompiler;
            _dtoConceptConverter = new ConceptDtoConverter();
        }

         // TODO: В будущем метод должен обновлять концепты для одного конкретного прочтения курса
        // TODO: Оптимизировать запросы, убрать лишние словари и сеты.
        public async Task SaveDefsAsync(ConceptManager conceptManager)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get current state from database
                var existingApprLists = await _context.AppraisalLists
                    .Include(a => a.AppraisalListColumns)
                        .ThenInclude(c => c.AppraisalListColumnsCompute)
                            .ThenInclude(c => c.ColumnDeps)
                    .AsNoTracking()
                    .ToDictionaryAsync(a => a.SourceIdentifier);
                var targetConcepts = conceptManager.ConceptDefs.ToDictionary(c => c.Key, c => c.Value);
                targetConcepts.Remove("Student");
                var existingColumns = existingApprLists.Values
                    .SelectMany(a => a.AppraisalListColumns)
                    .ToDictionary(c => (c.AppraisalList.SourceIdentifier, c.SourceIdentifier));
                var targetAttrs = targetConcepts.Values
                    .SelectMany(c => c.Attributes.Values)
                    .ToDictionary(c => (c.ParentConcept.Name, c.Name));

                // Find concepts and columns to add/update and delete
                var existingConceptSourceIds = existingApprLists.Values.Select(a => a.SourceIdentifier).ToHashSet();
                var targetConceptSourceIds = targetConcepts.Keys.ToHashSet();
                var existingColumnsSourceIds = existingApprLists.Values
                    .SelectMany(a => a.AppraisalListColumns)
                    .Select(c => (c.AppraisalList.SourceIdentifier, c.SourceIdentifier))
                    .ToHashSet();
                var targetAttrSourceIds = targetConcepts.Values
                    .SelectMany(c => c.Attributes.Values)
                    .Select(c => (c.ParentConcept.Name, c.Name))
                    .ToHashSet();

                var conceptsToAdd = targetConceptSourceIds.Except(existingConceptSourceIds).ToHashSet();
                var conceptsToDelete = existingConceptSourceIds.Except(targetConceptSourceIds).ToHashSet();
                var columnsToAddOrUpdate = targetAttrSourceIds;
                var columnsToDelete = existingColumnsSourceIds.Except(targetAttrSourceIds).ToHashSet();

                //TODO: тут будет удаление по условию пренадлежности к прочтению курса
                //Removing current computable columns
                var removeDeps = @"DELETE FROM CourseReport.AppraisalListColumnsCompute";
                await _context.Database.ExecuteSqlRawAsync(removeDeps);

                // Handle column deletions
                if (columnsToDelete.Any())
                {
                    var whereClauses = columnsToDelete
                        .Select((c, i) => $"(al.SourceIdentifier = @p{i * 2} AND alc.SourceIdentifier = @p{i * 2 + 1})");
                    var whereClause = string.Join(" OR ", whereClauses);

                    var deleteColumnsSql = $@"
                        DELETE alc
                        FROM CourseReport.AppraisalListColumns alc
                        JOIN CourseReport.AppraisalLists al ON al.Id = alc.AppraisalListId
                        WHERE {whereClause}";

                    var parameters = columnsToDelete.SelectMany(c => new object[] { c.Item1, c.Item2 }).ToArray();

                    await _context.Database.ExecuteSqlRawAsync(deleteColumnsSql, parameters);
                }

                // Handle concept deletions
                if (conceptsToDelete.Any())
                {
                    var deleteConceptsSql = @"
                        DELETE FROM CourseReport.AppraisalLists 
                        WHERE SourceIdentifier IN ({0})";
                    var parameters = string.Join(",", conceptsToDelete.Select((_, i) => $"@p{i}"));
                    await _context.Database.ExecuteSqlRawAsync(
                        string.Format(deleteConceptsSql, parameters),
                        conceptsToDelete.Select(c => (object)c).ToArray());
                }

                // Handle concept additions
                if (conceptsToAdd.Any())
                {
                    var insertConceptsSql = @"
                        INSERT INTO CourseReport.AppraisalLists (SourceIdentifier)
                        VALUES {0}";
                    var values = string.Join(",", conceptsToAdd.Select((_, i) => $"(@p{i})"));
                    var parameters = conceptsToAdd.Select(c => (object)c).ToArray();
                    await _context.Database.ExecuteSqlRawAsync(
                        string.Format(insertConceptsSql, values),
                        parameters);
                }

                var apprListIdsAfterUpdate = await _context.AppraisalLists
                    .AsNoTracking()
                    .ToDictionaryAsync(a => a.SourceIdentifier, a => a.Id);

                // Handle column additions and updates
                foreach (var (conceptSourceId, columnSourceId) in columnsToAddOrUpdate)
                {
                    var attr = targetAttrs[(conceptSourceId, columnSourceId)];
                    var isComputed = attr is ComputableAttributeDef;

                    var apprListId = apprListIdsAfterUpdate[conceptSourceId];
                    var upsertColumnSql = @"
                        MERGE CourseReport.AppraisalListColumns AS target
                        USING (
                            SELECT @p0 AS AppraisalListId, @p1 AS SourceIdentifier
                        ) AS source
                        ON target.AppraisalListId = source.AppraisalListId 
                           AND target.SourceIdentifier = source.SourceIdentifier
                        WHEN MATCHED THEN
                            UPDATE SET 
                                IsComputed = @p2
                        WHEN NOT MATCHED THEN
                            INSERT (AppraisalListId, SourceIdentifier, IsComputed)
                            VALUES (@p0, @p1, @p2);";

                    await _context.Database.ExecuteSqlRawAsync(
                        upsertColumnSql,
                        apprListId,
                        columnSourceId,
                        isComputed);
                }

                var columnsIdsAfterUpdate = await _context.AppraisalListColumns
                    .Include(c => c.AppraisalList)
                    .AsNoTracking()
                    .ToDictionaryAsync(c => (c.AppraisalList.SourceIdentifier, c.SourceIdentifier), c => c.Id);

                // Handle computable column dependencies
                foreach (var (conceptSourceId, columnSourceId) in columnsToAddOrUpdate)
                {
                    var attr = targetAttrs[(conceptSourceId, columnSourceId)];
                    if (attr is ComputableAttributeDef computableAttr)
                    {

                        //Insert computable column
                        var insertComputable = @"
                                INSERT INTO CourseReport.AppraisalListColumnsCompute (ColumnId, Operation)
                                VALUES (@p0, @p1)";
                            await _context.Database.ExecuteSqlRawAsync(
                                insertComputable,
                                columnsIdsAfterUpdate[(conceptSourceId, columnSourceId)],
                                computableAttr.Expr.SourceString);

                        // Insert new dependencies
                        if (computableAttr.Dependencies.Any())
                        {
                            // Insert column dependencies
                            var insertColumnDepsSql = @"
                                INSERT INTO CourseReport.AppraisalListColumnsDependencies (ColumnId, ColumnDepID)
                                VALUES (@p0, @p1)";
                            
                            foreach (var dep in computableAttr.Dependencies)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    insertColumnDepsSql,
                                    columnsIdsAfterUpdate[(conceptSourceId, columnSourceId)],
                                    columnsIdsAfterUpdate[(dep.ParentConcept.Name, dep.Name)]);
                            }
                        }
                    }
                }

                var students = await _context.Persons.ToListAsync();
                var colmnsToUpdate = existingColumnsSourceIds.Intersect(targetAttrSourceIds).ToHashSet();
                foreach (var col in colmnsToUpdate)
                {
                    if 
                    (
                        targetAttrs[col] is ComputableAttributeDef computableAttribute &&
                        (
                            !existingColumns[col].IsComputed ||
                            computableAttribute.Expr.SourceString != existingColumns[col].AppraisalListColumnsCompute.Operation
                        )
                    )
                    {
                        foreach (var student in students)
                        {
                            var score = await GetRecalcedScoreAsync(computableAttribute, student.Id);
                            await SaveOneMarkScoreAsync(computableAttribute, student.Id, score);
                        }
                    }
                }

                var columnsToAdd = targetAttrSourceIds.Except(existingColumnsSourceIds).ToHashSet();
                foreach (var col in columnsToAdd)
                {
                    if (targetAttrs[col] is ComputableAttributeDef computableAttribute)
                    {
                        foreach (var student in students)
                        {
                            var score = await GetRecalcedScoreAsync(computableAttribute, student.Id);
                            await SaveOneMarkScoreAsync(computableAttribute, student.Id, score);
                        }
                    }
                }
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SaveMarksAsync(ConceptDef apprListConceptDef)
        {
            if (apprListConceptDef.Name == "Student")
            {
                throw new NotSupportedException("Saving student concept as appraisal list isn't supported");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var attr in apprListConceptDef.Attributes.Values.Where(attrDef => attrDef is AttributeDef).Cast<AttributeDef>())
                {
                    await SaveMarksColumnAsync(attr);
                }
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ConceptDef> LoadMarksAsync(ConceptDef apprListConceptDef)
        {
            var apprListConceptAttrs = apprListConceptDef.Attributes.Values;
            var deps = apprListConceptAttrs
                .Where(a => a is ComputableAttributeDef)
                .Cast<ComputableAttributeDef>()
                .SelectMany(a => a.Dependencies)
                .ToList();

            foreach (var attr in apprListConceptAttrs)
            {
                await LoadRowAsync(attr);
            }

            foreach (var dep in deps)
            {       
                await LoadRowAsync(dep);
            }

            return apprListConceptDef;
        }

        // TODO: В будущем метод должен обновлять концепты для одного конкретного прочтения курса
        public async Task<ConceptManager> LoadApprListDefsAsync()
        {
            var apprLists = await _context.AppraisalLists
                .Include(apprList => apprList.AppraisalListColumns)
                    .ThenInclude(col => col.AppraisalListColumnsCompute)
                .AsNoTracking()
                .ToListAsync();

            var conceptDefsDto = ConvertApprListsToConceptDto(apprLists);
            var conceptDefs = _dtoConceptConverter.DtoToDefinitions(conceptDefsDto);
            DefineStudentConcept(conceptDefs);
            return conceptDefs;
        }

        //TODO: опять же студенты относящиеся к определённому прочтению курса
        //FIXME: ВНИМАНИЕ!!! Эта штука сейчас не идемпотентная, если загрузить студентов дважды, то каждый студент будет по два раза 
        public async Task<ConceptManager> LoadStudentsAsync(ConceptManager conceptManager)
        {
            var students = await _context.Persons
                .AsNoTracking()
                .OrderBy(s => s.Id) //и так отсортированы по id?
                .ToListAsync();
            foreach (var student in students)
            {
                conceptManager.CreateInstanceGroup().CreateInstance("Student")
                    .SetAttribute("id", student.Id)
                    .SetAttribute("firstName", student.FirstName)
                    .SetAttribute("middleName", student.MiddleName)
                    .SetAttribute("lastName", student.LastName)
                    .SetAttribute("birthDate", student.BirthDate);
            }
            return conceptManager;
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }

        private static ConceptDef DefineStudentConcept(ConceptManager conceptManager)
        {
            var studentConcept = conceptManager.DefineConcept("Student")
                .AddAttribute("id", typeof(Guid))
                .AddAttribute("firstName", typeof(string))
                .AddAttribute("middleName", typeof(string))
                .AddAttribute("lastName", typeof(string))
                .AddAttribute("birthDate", typeof(DateTime));
            return studentConcept;
        }

        private ConceptDefCollectionDto ConvertApprListsToConceptDto(List<AppraisalList> apprLists)
        {
            var result = new ConceptDefCollectionDto();

            foreach (var apprList in apprLists)
            {
                var conceptDto = new ConceptDefDto
                {
                    Name = apprList.SourceIdentifier,
                    Attributes = new List<AttributeDefDto>()
                };

                foreach (var col in apprList.AppraisalListColumns)
                {
                    var attrDto = new AttributeDefDto
                    {
                        Name = col.SourceIdentifier,
                        Type = "Int", // Пока так, потом придумаю как хранить тип в бд
                        IsComputable = col.IsComputed,
                        Expression = col.AppraisalListColumnsCompute?.Operation
                    };

                    conceptDto.Attributes.Add(attrDto);
                }

                result.Concepts.Add(conceptDto);
            }

            return result;
        }

        private async Task<ConceptManager> LoadRowAsync(AttributeDefBase attrDef)
        {
            var conceptManager = attrDef.ParentConcept.ConceptManager;
            var columnMarks = await (
                from person in _context.Persons 
                from mark in person.AppraisalListMarks.DefaultIfEmpty()
                where mark == null ||
                mark.AppraisalListColumn.SourceIdentifier == attrDef.Name && 
                mark.AppraisalListColumn.AppraisalList.SourceIdentifier == attrDef.ParentConcept.Name
                orderby person.Id
                select mark
                ).ToListAsync();

            // var columnMarks = await _context.AppraisalListMarks
            //     .Where(
            //         m => m.AppraisalListColumn.SourceIdentifier == attrDef.Name &&
            //         m.AppraisalListColumn.AppraisalList.SourceIdentifier == attrDef.ParentConcept.Name
            //     )
            //     .OrderBy(m => m.StudentPersonId)
            //     .ToListAsync();

            foreach (var (group, mark) in conceptManager.InstanceGroups.Zip(columnMarks))
            {
                group
                    .GetOrCreateInstance(attrDef.ParentConcept)
                    .SetAttributeCachedValue(attrDef.Name, mark?.Score);
            }

            return conceptManager;
        }


        private async Task<object> GetRecalcedScoreAsync(ComputableAttributeDef compAttrDef, Guid studentId)
        {
            //manmade horror beyond comprehension
            var dependeciesMarksScore = await _context.AppraisalListColumns
                .Where( thisColumn => thisColumn.SourceIdentifier == compAttrDef.Name &&
                        thisColumn.AppraisalList.SourceIdentifier == compAttrDef.ParentConcept.Name)
                .SelectMany(thisColumn => thisColumn.AppraisalListColumnsCompute!.ColumnDeps)
                .SelectMany(dependencyCol => dependencyCol.AppraisalListMarks)
                .Where(dependencyMark => dependencyMark.StudentPersonId == studentId)
                .Include(dependencyMark => dependencyMark.AppraisalListColumn)
                    .ThenInclude(dependencyCol => dependencyCol.AppraisalList)
                .ToDictionaryAsync
                (
                    dependencyMark => (dependencyMark.AppraisalListColumn.AppraisalList.SourceIdentifier, dependencyMark.AppraisalListColumn.SourceIdentifier),
                    dependencyMark => dependencyMark.Score
                );

            // if(compAttrDef.Name == "score" && compAttrDef.ParentConcept.Name == "Lab1" && Guid.Parse("4af14b87-0dc4-4ac7-917e-419c66569a85") == studentId)
            // {
            //     foreach (var depMarkScore in dependeciesMarksScore)
            //     {
            //         Console.WriteLine($"{depMarkScore.Key.Item1}.{depMarkScore.Key.Item2} = {depMarkScore.Value}");
            //     }
            //     Console.WriteLine("");
            // }
                
            var args = compAttrDef.Dependencies.Select
            (
                d => 
                {
                    int? ret = dependeciesMarksScore.TryGetValue((d.ParentConcept.Name, d.Name), out var outVar) ? outVar : null;
                    return (object?)ret;
                }
            ).ToArray();
            return compAttrDef.Expr.Func(args);
        }

        private async Task<(AppraisalListMark mark, bool createdNew)> GetMarkForAttrAsync(AttributeDefBase attrDef, Guid studentId)
        {
            string apprListSourceId = attrDef.ParentConcept.Name;
            string apprListColSourceId = attrDef.Name;
            bool createdNew = false;
            var depMark = await _context
                    .AppraisalListMarks
                    .SingleOrDefaultAsync(
                        m => m.StudentPersonId == studentId &&
                        m.AppraisalListColumn.SourceIdentifier == apprListColSourceId &&
                        m.AppraisalListColumn.AppraisalList.SourceIdentifier == apprListSourceId
                    );
                if (depMark == null)
                {
                    depMark = new AppraisalListMark {
                        StudentPersonId = studentId,
                        AppraisalListColumnId = await _context.AppraisalListColumns
                            .Where
                            (
                                c => c.SourceIdentifier == apprListColSourceId &&
                                c.AppraisalList.SourceIdentifier == apprListSourceId
                            )
                            .Select(c => c.Id)
                            .FirstAsync()
                    };
                    _context.Add(depMark);
                    createdNew = true;
                }
            return (depMark, createdNew);
        }

        //TODO: сделать без рекурсии
        // АаааААаааааааААааааааАААааааааааа
        //https://youtu.be/fxKTXoDV74Q?si=12B09U0Wl_8Cjdkt
        private async Task SaveOneMarkScoreAsync(AttributeDefBase attrDef, Guid studentId, object value)
        {
            (var mark, bool createdNew) = await GetMarkForAttrAsync(attrDef, studentId);
            if (!createdNew && mark.Score == (int?)value)
            {
                return;
            }
            // if (attrDef.Name == "testsScore" && Guid.Parse("4af14b87-0dc4-4ac7-917e-419c66569a85") == studentId)
            // {
            //     foreach (var depAttrDef in attrDef.DependentAttrs)
            //     {
            //         Console.WriteLine($"{depAttrDef.ParentConcept.Name}.{depAttrDef.Name}");
            //     }
            //     Console.WriteLine("");
            // }
            mark.Score = (int?)value; //TODO: поддержка других типов
            await _context.SaveChangesAsync();
            foreach (var depAttrDef in attrDef.DependentAttrs)
            {
                var depMarkValue = await GetRecalcedScoreAsync((ComputableAttributeDef)depAttrDef, studentId);
                await SaveOneMarkScoreAsync(depAttrDef, studentId, depMarkValue);
            }
        }

        private async Task SaveMarksColumnAsync(AttributeDef attrDef)
        {
            var attrs = attrDef.GetInstances();

            foreach (var attr in attrs)
            {
                Guid studentId = (Guid)attr
                    .ParentConcept
                    .Group
                    .GetInstance("Student")
                    .GetAttributeValue("id");
                var value = attr.Value;
                await SaveOneMarkScoreAsync(attrDef, studentId, value);
            }
        }

        // public async Task CommitAsync()
        // {
        //     await _transaction.CommitAsync();
        //     _transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        // }

        // public async Task RollbackAsync()
        // {
        //     await _transaction.RollbackAsync();
        //     _transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        // }

    }
}
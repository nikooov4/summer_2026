using Mephi.Cybernetics.Race.AppraisalListsMapping;
using Mephi.Cybernetics.Race.AppraisalListsMapping.Models;
using Mephi.Cybernetics.Race.MappingDescriptionLanguage;
using Mephi.Cybernetics.Race.AppraisalListsMapping.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Drawing;
using System.Linq;


namespace Mephi.Cybernetics.Race.AppraisalListsMapping
{
    internal class Program
    {
        public static Dictionary<Operations, ColumnOperations> OpertationsDict = new Dictionary<Operations, ColumnOperations>()
            {
                { Operations.SUM, new ColumnOperations{ ID="SUM", Description="Суммирование" } },
                { Operations.MIN, new ColumnOperations{ ID="MIN", Description="Минимальное значение" } },
                { Operations.EnumRange, new ColumnOperations{ ID=Operations.EnumRange.ToString(), Description="Enum для диапаона" } },
            };

        public static class Consts
        {
            public static List<Guid> Students { get; set; }
            public const int Amount = 10;
            public const string SourceName = "vector";
            public static readonly Guid Teacher = new Guid();

            static Consts()
            {
                Students = new List<Guid>(Amount);
                for (int i = 0; i < Amount; i++)
                {
                    Students.Add(Guid.NewGuid());
                }
            }
        }

        public static List<InterAppraisalListMark> GenerateRandomMarks(InterAppraisalListColumn column)
        {
            var marks = new List<InterAppraisalListMark>();
            for (int i = 0; i < Consts.Amount; i++)
            {
                var oldScore = Random.Shared.Next(column.MaxScore / 3, column.MaxScore);
                var score = Random.Shared.Next(column.MaxScore / 3, column.MaxScore);
                marks.Add(new InterAppraisalListMark() { OldScore = oldScore, AppraisalListColumnID = column.ID, OldScoreSourceName = Consts.SourceName, OldTeacherPersonID = new Guid(), Score = score, ScoreSourceName = Consts.SourceName, UpdatedAt = DateTime.Now, TeacherPersonID = new Guid() });
            }
            return marks;
        }

        public static List<AppraisalListMarks> GenerateRandomMarks(AppraisalListColumns column)
        {
            var marks = new List<AppraisalListMarks>();
            for (int i = 0; i < Consts.Amount; i++)
            {
                var oldScore = Random.Shared.Next(column.MaxScore / 3, column.MaxScore);
                var score = Random.Shared.Next(column.MaxScore / 3, column.MaxScore);
                marks.Add(new AppraisalListMarks() { OldScore = oldScore, AppraisalListColumnID = column.ID, _column = column, OldScoreSourceName = Consts.SourceName, OldTeacherPersonID = new Guid(), Score = score, ScoreSourceName = Consts.SourceName, StudentPersonID = Consts.Students[i], UpdatedAt = DateTime.Now, TeacherPersonID = new Guid() });
            }
            return marks;
        }

        public static void ComputeColumn(AppraisalListColumns column)
        {
            if (!column.IsComputed)
                return;
            var columns = column._dependsFrom.Select((c, i) => c._dependsFrom._marks.Select(m => m.Score).ToList()).ToList();
            var students = column._dependsFrom[0]._dependsFrom._marks.Select(m => m.StudentPersonID).ToArray();
            var rows = LanguageUtils.Transpose(columns);
            var agg = AppraisalOperations.Get(column._computeColumns[0]._operation.ID);
            var computedMarks = rows.Select(r => agg(r.Zip(column._dependsFrom).ToList())).Cast<int>();
            column._marks = new List<AppraisalListMarks>();
            int i = 0;
            foreach ( var mark in computedMarks) 
            {
                column._marks.Add(new AppraisalListMarks() { _column=column, AppraisalListColumnID=column.ID, OldScore=null, OldScoreSourceName=null, OldTeacherPersonID=null, Score=mark, ScoreSourceName="System", StudentPersonID= students[i], TeacherPersonID=null, UpdatedAt=DateTime.Now});
                i++;
            }
        }

        public static void ComputeColumn(InterAppraisalListColumn column)
        {
            if (column.Computation == null)
                return;
            foreach(var dep in column.DependsFrom)
            {
                if (dep.Computation != null && dep.Marks == null)
                {
                    ComputeColumn(dep);
                }
            }
            var columns = column.DependsFrom.Select((c, i) => c.Marks.Select(m => m.Score).ToList()).ToList();
            var rows = LanguageUtils.Transpose(columns);
            var agg = AppraisalOperations.GetInter(column.Computation.OperationID);
            var computedMarks = rows.Select(r => agg(  (r.Zip(column.DependsFrom).ToList(), column))  ).Cast<int>();
            column.Marks = new List<InterAppraisalListMark>();
            int i = 0;
            foreach (var mark in computedMarks)
            {
                column.Marks.Add(new InterAppraisalListMark() { AppraisalListColumnID = column.ID, OldScore = null, OldScoreSourceName = null, OldTeacherPersonID = null, Score = mark, ScoreSourceName = "System", TeacherPersonID = null, UpdatedAt = DateTime.Now });
                i++;
            }
        }

        public static InterAppraisalScale MakeInterEnumScale(List<(int code, string desc, string name)> data)
        {
            var scale2Id = Guid.NewGuid();
            var scaleEnum = new InterAppraisalScale
            {
                Description = $"Описание шкалы {scale2Id}",
                Name = $"Enum шкала {scale2Id}",
                ID = scale2Id,
                EnumValues = data.Select(d => new InterAppraisalScalesEnumValue() { Code = d.code, Description = d.desc, Name = d.name}).ToList()
            };
            return scaleEnum;
        }

        public static List<InterAppraisalScalesEnumRange> RangeFromScale(InterAppraisalScale scale, List<(int min, int max)> ranges)
        {
            var res = new List<InterAppraisalScalesEnumRange>();

            int i = 0;
            foreach (var e in scale.EnumValues)
            {
                var range = new InterAppraisalScalesEnumRange() { RangeStart = ranges[i].min, RangeEnd = ranges[i].max, EnumValue=e };
                res.Add(range);
                i++;
            }
            return res;
        }

        public static AppraisalScales MakeEnumScale(List<(int code, string desc, string name)> data)
        {
            var scale2Id = Guid.NewGuid();
            var scaleEnum = new AppraisalScales
            {
                Description = $"Описание шкалы {scale2Id}",
                IsEnum = true,
                IsLinear = false,
                Name = $"Enum шкала {scale2Id}",
                ID = scale2Id,
                _linearScales = null,
                _enumValues = data.Select(d => new AppraisalScalesEnumValues() { Code = d.code, Description = d.desc, Name = d.name, ScaleID = scale2Id }).ToList()
            };
            for (int i = 0; i < scaleEnum._enumValues.Count; i++)
                scaleEnum._enumValues[i]._scale = scaleEnum;
            return scaleEnum;
        }

        public static void InitColumnWithRange(AppraisalScales scale, AppraisalListColumnsCompute column, List<(int min, int max)> intranges)
        {
            var ranges = new List<AppraisalScalesEnumRanges>();

            int i = 0;
            foreach (var e in scale._enumValues)
            {
                var range = new AppraisalScalesEnumRanges() { ComputeColumnID = column.ColumnID, EnumValueID = e.ScaleID, _column = column, _enumValue = e, RangeStart = intranges[i].min, RangeEnd = intranges[i].max };
                e._ranges = [range];
                ranges.Add(range);
                i++;
            }
            column._ranges = ranges;
        }

        public static AppraisalListColumns CreateColumn(int order, AppraisalScales scale, string name, Operations? operation, (int maxScore, int minAcceptScore) score, List<AppraisalListColumns> dependencies, List<(int min, int max)> intranges, bool fillMarks=true)
        {
            var columnId = Guid.NewGuid();
            var column = new AppraisalListColumns
            {
                Name = name,
                Comment = $"Комментарий к '{name}'",
                CourseInstanceModuleID = null,
                Description = $"Описание к '{name}'",
                IsComputed = operation != null,
                MaxScore = score.maxScore,
                MinAcceptScore = score.minAcceptScore,
                OrderIndex = order,
                ScaleID = scale.ID,
                ID = columnId,
                _computeColumns = null,
                _scale = scale,
                _dependsFrom = null,
            };
            if(scale._columns == null)
                scale._columns = new List<AppraisalListColumns>();
            scale._columns.Add(column);

            if (column.IsComputed)
            {
                column._computeColumns = new List<AppraisalListColumnsCompute>() { new AppraisalListColumnsCompute() { ColumnID = column.ID, OperationID = OpertationsDict[operation.Value].ID, _column = column, _operation = OpertationsDict[operation.Value]} };
                column._dependsFrom = dependencies.Select(d => new AppraisalListColumnsDependencies() { ColumnID = column.ID, _column = column, _dependsFrom = d, ColumnDepID = d.ID }).ToList();
                foreach(var dep in column._dependsFrom)
                {
                    if (dep._dependsFrom._dependedBy == null)
                        dep._dependsFrom._dependedBy = new List<AppraisalListColumnsDependencies>();
                    dep._dependsFrom._dependedBy.Add(dep);
                }
                if (operation.Value == Operations.EnumRange)
                {
                    InitColumnWithRange(scale, column._computeColumns[0], intranges);
                }
            }
            if (column._scale.IsLinear && !column.IsComputed && fillMarks)
                column._marks = GenerateRandomMarks(column);

            return column;
        }

        public static List<AppraisalListColumns> GetColumns() {
            var scale1Id = Guid.NewGuid();
            var scaleLinear = new AppraisalScales
            {
                Description = $"Описание шкалы {scale1Id}",
                IsEnum = false,
                IsLinear = true,
                Name = "Линейная шкала",
                ID = scale1Id,
                _linearScales = new List<AppraisalScalesLinear>() { new AppraisalScalesLinear() { ID=scale1Id, MaxValue=100, MinValue=0, ValueFormat="?"} },
                _enumValues = null
            };
            scaleLinear._linearScales[0]._scale = scaleLinear;

            var scaleEnumAttestation = MakeEnumScale([(0, "Не аттестован", "н/a"), (1, "Аттестован", "а")]);
            var scaleEnumRusMark = MakeEnumScale([(0, "Неудовлетварительно", "неуд."), (1, "Удовлетварительно", "удовл."), (2, "Хорошо", "хор."), (3, "Отлично", "отл.")]);
            var scaleEnumEnMark = MakeEnumScale([(0, "F", "F"), (1, "E", "E"), (2, "D", "D"), (3, "C", "C"), (4, "B", "B"), (5, "A", "A")]);

            var column1 = CreateColumn(1, scaleLinear, "Раздел 1", null, (35, 20), null, null);
            var column2 = CreateColumn(2, scaleLinear, "Раздел 2", null, (35, 20), null, null);
            var columnIn = CreateColumn(3, scaleLinear, "Раздел 3", null, (30, 20), null, null);
            var column3 = CreateColumn(4, scaleLinear, "Сумма", Operations.SUM, (100, 60), [column1, column2, columnIn], null);
            var column4 = CreateColumn(5, scaleEnumAttestation, "Аттестация", Operations.EnumRange, (scaleEnumAttestation._enumValues[^1].Code, scaleEnumAttestation._enumValues[^1].Code), [column3], [(0, 59), (60, 100)]);
            var column5 = CreateColumn(6, scaleEnumRusMark, "Оценка", Operations.EnumRange, (scaleEnumRusMark._enumValues[^1].Code, 1), [column3], [(0, 59), (60, 69), (70, 89), (90,100)]);
            var column6 = CreateColumn(7, scaleEnumEnMark, "ECTS", Operations.EnumRange, (scaleEnumAttestation._enumValues[^1].Code, 1), [column3], [(0, 59), (60, 64), (65, 69), (70, 79), (80, 89), (90, 100)]);

            return new List<AppraisalListColumns> { column1, column2, columnIn, column3, column4, column5, column6 };
        }

        static void PrintTable(string[,] data)
        {
            int numRows = data.GetLength(0);
            int numCols = data.GetLength(1);

           
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    if (data[row, col] == null)
                    {
                        data[row, col] = "null";
                    }
                }
            }

            // Calculate column widths
            int[] columnWidths = new int[numCols];
            for (int col = 0; col < numCols; col++)
            {
                int maxWidth = 0;
                for (int row = 0; row < numRows; row++)
                {
                    int length = data[row, col] == null ? 0 : data[row, col].Length;
                    if (length > maxWidth)
                        maxWidth = length;
                }
                columnWidths[col] = maxWidth;
            }

            // Print table
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    Console.Write(data[row, col].PadRight(columnWidths[col] + 2));
                }
                Console.WriteLine();
            }
        }

        public static void PrintAppraisalList(AppraisalLists list)
        {
            var cols = list._columns.OrderBy(c => c.OrderIndex).ToList();
            var notComputableCols = cols.Where(c => c.IsComputed).ToList();
            var rowsAmount = notComputableCols.First()._marks.Count + 1;
            var rows = new string[rowsAmount, cols.Count() + 1];

            rows[0, 0] = "Студент";
            int colIdx = 1;
            for (colIdx = 0; colIdx < cols.Count; colIdx++) 
            {
                rows[0, colIdx+1] = cols[colIdx].Name;
            }

            colIdx = 1;
            foreach (var col in cols)
            {
                if (!col._scale.IsEnum)
                {
                    for (int rowIdx = 1; rowIdx < rowsAmount; rowIdx++)
                    {
                        rows[rowIdx, colIdx] = col._marks[rowIdx - 1].Score.ToString();
                    }
                }else
                {
                    for (int rowIdx = 1; rowIdx < rowsAmount; rowIdx++)
                    {
                        rows[rowIdx, colIdx] = col._scale._enumValues.First(e => e.Code == col._marks[rowIdx - 1].Score).Name;
                    }
                }
                colIdx++;
            }



            Console.WriteLine($"|{list.Name}|");
            PrintTable(rows);
        }

        public static void PrintAppraisalList(AttestationAppraisalList list)
        {
            var cols = list.GetType().GetProperties().Where(p => p.PropertyType == typeof(InterAppraisalListColumn)).Select(p => (InterAppraisalListColumn)p.GetValue(list)).OrderBy(c => c.OrderIndex).ToList();
            var notComputableCols = cols.Where(c => c.Computation != null).ToList();
            var rowsAmount = notComputableCols.First().Marks.Count + 1;
            var rows = new string[rowsAmount, cols.Count() + 1];

            rows[0, 0] = "Студент";
            int colIdx = 1;
            for (colIdx = 0; colIdx < cols.Count; colIdx++)
            {
                rows[0, colIdx + 1] = cols[colIdx].Name;
            }

            for (int rowIdx = 1; rowIdx < rowsAmount; rowIdx++)
            {
                rows[rowIdx, 0] = list.Students[rowIdx-1].Substring(0, 5);
            }

            colIdx = 1;
            foreach (var col in cols)
            {
                if (col.Scale.EnumValues == null)
                {
                    for (int rowIdx = 1; rowIdx < rowsAmount; rowIdx++)
                    {
                        rows[rowIdx, colIdx] = col.Marks[rowIdx - 1].Score.ToString();
                    }
                }
                else
                {
                    for (int rowIdx = 1; rowIdx < rowsAmount; rowIdx++)
                    {
                        rows[rowIdx, colIdx] = col.Scale.EnumValues.First(e => e.Code == col.Marks[rowIdx - 1].Score).Name;
                    }
                }
                colIdx++;
            }



            Console.WriteLine($"|{list.Name}|");
            PrintTable(rows);
        }

        public static void PrintAppraisalList(MarksCountAppraisalList list)
        {
            var cols = list.GetType().GetProperties().Where(p => p.PropertyType == typeof(InterAppraisalListColumn)).Select(p => (InterAppraisalListColumn)p.GetValue(list)).Where(v => v != null).OrderBy(c => c.OrderIndex).ToList();
            var notComputableCols = cols.Where(c => c.Computation == null).ToList();
            var rowsAmount = notComputableCols.First().Marks.Count + 1;
            var rows = new string[rowsAmount, cols.Count() + 1];

            rows[0, 0] = "Студент";
            int colIdx = 1;
            for (colIdx = 0; colIdx < cols.Count; colIdx++)
            {
                rows[0, colIdx + 1] = cols[colIdx].Name;
            }

            for (int rowIdx = 1; rowIdx < rowsAmount; rowIdx++)
            {
                rows[rowIdx, 0] = list.Students[rowIdx - 1].Substring(0, 5);
            }

            colIdx = 1;
            foreach (var col in cols)
            {
                if (col.Scale.EnumValues == null)
                {
                    for (int rowIdx = 1; rowIdx < rowsAmount; rowIdx++)
                    {
                        rows[rowIdx, colIdx] = col.Marks[rowIdx - 1].Score.ToString();
                    }
                }
                else
                {
                    for (int rowIdx = 1; rowIdx < rowsAmount; rowIdx++)
                    {
                        rows[rowIdx, colIdx] = col.Scale.EnumValues.First(e => e.Code == col.Marks[rowIdx - 1].Score).Name;
                    }
                }
                colIdx++;
            }

            Console.WriteLine($"|{list.Name}|");
            PrintTable(rows);
        }

        

        public static AppraisalLists GetAppraisal(Guid courseID, string name, List<AppraisalListColumns> cols)
        {
            var appraisalID = Guid.NewGuid();
            AppraisalLists appraisal = new()
            {
                Comment = $"Комментарий к ведомости '{name}'",
                CourseID = courseID,
                CourseProgramID = null,
                DateClosed = DateTime.Today,
                DateCreated = DateTime.Today - TimeSpan.FromDays(10),
                Description = $"Описание ведомости '{name}'",
                ID = appraisalID,
                Name = name,
                Year = 2024,
            };

            if (cols == null)
                cols = GetColumns();
            appraisal._columns = cols;
            foreach(var col in cols)
            {
                col.AppraisalListID = appraisal.ID;
                col._list = appraisal;
            }

            foreach (var col in appraisal._columns.Where(c => c.IsComputed))
            {
                ComputeColumn(col);
            }
            return appraisal;
        }


        public static void MapAppraisalLists(List<AppraisalLists> source)
        {
            var scale1Id = Guid.NewGuid();
            var scaleLinear = new AppraisalScales
            {
                Description = $"Описание шкалы {scale1Id}",
                IsEnum = false,
                IsLinear = true,
                Name = "Линейная шкала",
                ID = scale1Id,
                _linearScales = new List<AppraisalScalesLinear>() { new AppraisalScalesLinear() { ID = scale1Id, MaxValue = 100, MinValue = 0, ValueFormat = "?" } },
                _enumValues = null
            };
            scaleLinear._linearScales[0]._scale = scaleLinear;

            var scaleEnumAttestation = MakeEnumScale([(0, "Не аттестован", "н/a"), (1, "Аттестован", "а")]);
            var scaleEnumRusMark = MakeEnumScale([(2, "Число двоек", "2") ,(3, "Число троек", "3"), (4, "Число четверок", "4"), (5, "Число пятерок", "5")]);

            var column1 = CreateColumn(1, scaleLinear, "2", null, (2, 0), null, null, false);
            var column2 = CreateColumn(2, scaleLinear, "3", null, (2, 0), null, null, false);
            var column3 = CreateColumn(3, scaleLinear, "4", null, (2, 0), null, null, false);
            var column4 = CreateColumn(4, scaleLinear, "5", null, (2, 0), null, null, false);

         
            var cols =  new List<AppraisalListColumns> { column1, column2, column3, column4 };

            var dest = GetAppraisal(new Guid(), "Итог", cols);
        }


        public static AttestationAppraisalList GenerateAttestationAppraisalList(string name, Guid listId, List<InterAppraisalScalesEnumRange> ranges, InterAppraisalScale scaleEnumAttestation, InterAppraisalScale scaleLinear)
        {

            var partCol1 = new InterAppraisalListColumn() { ID = Guid.NewGuid(), Comment = "ajasdlkfjsdf", Computation = null, AppraisalListID = listId, Description = "asjdfl", CourseInstanceModuleID = null, DependsFrom = null, MaxScore = 50, MinAcceptScore = 30, Name = "Раздел 1", OrderIndex = 1, Scale = scaleLinear };
            partCol1.Marks = GenerateRandomMarks(partCol1);
            var partCol2 = new InterAppraisalListColumn() { ID = Guid.NewGuid(), Comment = "sdklfjlafjsdf", Computation = null, AppraisalListID = listId, Description = "lkasjdfl", CourseInstanceModuleID = null, DependsFrom = null, MaxScore = 50, MinAcceptScore = 30, Name = "Раздел 2", OrderIndex = 2, Scale = scaleLinear };
            partCol2.Marks = GenerateRandomMarks(partCol2);

            var sumCol = new InterAppraisalListColumn() { ID = Guid.NewGuid(), Comment = "regvafsdlkfjsdf", Computation = new InterAppraisalListColumnsCompute() { OperationID = Operations.SUM, Ranges = null }, AppraisalListID = listId, Description = "t44gasjdfl", CourseInstanceModuleID = null, DependsFrom = [partCol1, partCol2], MaxScore = 100, MinAcceptScore = 60, Name = "Сумма", OrderIndex = 3, Scale = scaleLinear };

            var attCol = new InterAppraisalListColumn() { ID = Guid.NewGuid(), Comment = "tlkjregvafsdlkfjsdf", Computation = new InterAppraisalListColumnsCompute() { OperationID = Operations.EnumRange, Ranges = ranges }, AppraisalListID = listId, Description = "t44gasjdfl", CourseInstanceModuleID = null, DependsFrom = [sumCol], MaxScore = 1, MinAcceptScore = 1, Name = "Аттестация", OrderIndex = 4, Scale = scaleEnumAttestation };

            //var attCol = new InterAppraisalListColumn() { ID= Guid.NewGuid(), Comment="ajsdf", Computation= };
            var atterstationList = new AttestationAppraisalList() { ID = listId, Comment = "sklf", DateCreated = DateTime.Now, Year = 2024, DateClosed = DateTime.Now, Description = "asdfjls", Name = name, Students = Consts.Students.Select(s => s.ToString()).ToList(), Part1 = partCol1, Part2 = partCol2, Sum = sumCol, Attestation = attCol };
            return atterstationList;
        }


        public static void MapLists(List<AttestationAppraisalList> lists, MarksCountAppraisalList target)
        {
            //lists.Select()
        }

        public static void PrintOld()
        {
            var courseID = Guid.NewGuid();
            var appraisal1 = GetAppraisal(courseID, "Проектная практика", null);
            var appraisal2 = GetAppraisal(courseID, "Линейная алгебра", null);



            PrintAppraisalList(appraisal1);
            Console.WriteLine("\n------------\n");
            PrintAppraisalList(appraisal2);
        }

        static void Main(string[] args)
        {
 
            var attListId = new Guid();

            var scaleLinear = new InterAppraisalScale()
            {
                Description = $"Описание лин шкалы",
                Name = "Линейная шкала",
                ID = new Guid(),
                EnumValues = null,
                LinearScale = new InterAppraisalScaleLinear() { MaxValue = 100, MinValue = 0, ValueFormat = "?" }
            };
            //var scaleEnumAttestation = MakeInterEnumScale([(0, "Не аттестован", "н/a"), (1, "Аттестован", "а")]);
            //var ranges = RangeFromScale(scaleEnumAttestation, [(0, 59), (60, 100)]);


            var scaleEnumRusMark = MakeInterEnumScale([(0, "Неудовлетварительно", "неуд."), (1, "Удовлетварительно", "удовл."), (2, "Хорошо", "хор."), (3, "Отлично", "отл.")]);
            var ranges = RangeFromScale(scaleEnumRusMark, [(0, 59), (60, 69), (70, 89), (90, 100)]);
            var dmList1 = GenerateAttestationAppraisalList("ДМ1", attListId, ranges, scaleEnumRusMark, scaleLinear);
            var dmList2 = GenerateAttestationAppraisalList("ДМ2", attListId, ranges, scaleEnumRusMark, scaleLinear);
            var dmList3 = GenerateAttestationAppraisalList("ДМ3", attListId, ranges, scaleEnumRusMark, scaleLinear);
            var dmList = new List<AttestationAppraisalList> { dmList1, dmList2, dmList3 };
            foreach(var l in dmList)
            {
                ComputeColumn(l.Attestation);
                PrintAppraisalList(l);
                Console.WriteLine();
            }


            var allMarksList = new MarksCountAppraisalList() { ID = attListId, Comment = "sklf", DateCreated = DateTime.Now, Year = 2024, DateClosed = DateTime.Now, Description = "asdfjls", Name = "Сумма оценок", Students = dmList1.Students, Fives=new() { Scale=scaleLinear, Name="Пятерки"}, Fours = new() { Scale = scaleLinear, Name = "Четверки" } , Threes = new() { Scale = scaleLinear, Name = "Тройки" } , Twos = new() { Scale = scaleLinear, Name = "Двойки" } };

            string code = "@out.Fives.Marks = select(transpose(select(select(@in, l => l.Attestation), c => c.Marks)), r =>{AppraisalListColumnID: @out.Fives.ID, Score:count(filter(r, m => m.Score == 3)) })";
            var compiler = new CustomLanguageCompiler();
            var lambdaFives = compiler.Compile<List<AttestationAppraisalList>, MarksCountAppraisalList>(code);
            code = "@out.Fours.Marks = select(transpose(select(select(@in, l => l.Attestation), c => c.Marks)), r =>{AppraisalListColumnID: @out.Fives.ID, Score:count(filter(r, m => m.Score == 2)) })";
            var lambdaFours = compiler.Compile<List<AttestationAppraisalList>, MarksCountAppraisalList>(code);
            code = "@out.Threes.Marks = select(transpose(select(select(@in, l => l.Attestation), c => c.Marks)), r =>{AppraisalListColumnID: @out.Fives.ID, Score:count(filter(r, m => m.Score == 1)) })";
            var lambdaThrees = compiler.Compile<List<AttestationAppraisalList>, MarksCountAppraisalList>(code);
            code = "@out.Twos.Marks = select(transpose(select(select(@in, l => l.Attestation), c => c.Marks)), r =>{AppraisalListColumnID: @out.Fives.ID, Score:count(filter(r, m => m.Score == 0)) })";
            var lambdaTwos = compiler.Compile<List<AttestationAppraisalList>, MarksCountAppraisalList>(code);

            lambdaFives(dmList, allMarksList);
            lambdaFours(dmList, allMarksList);
            lambdaThrees(dmList, allMarksList);
            lambdaTwos(dmList, allMarksList);

            PrintAppraisalList(allMarksList);
        }
    }
}

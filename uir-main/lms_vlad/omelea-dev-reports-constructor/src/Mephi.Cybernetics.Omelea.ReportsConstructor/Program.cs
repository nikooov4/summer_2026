using Mephi.Cybernetics.Omelea.ReportsConstructor.Models;
using Mephi.Cybernetics.Omelea.ReportsConstructor;
using Microsoft.EntityFrameworkCore;
using Mephi.Cybernetics.Race.ReportsDescriptionLanguage;
using System.Text.Json;
using System.Threading.Tasks;

//TODO: Можно это превратить в тесты

static class Program
{
    static void PrintConcepts(ConceptDef conceptDef)
    {
        Console.WriteLine($"{conceptDef.Name}:");
        foreach (var concept in conceptDef.GetInstances())
        {
            Console.Write($"name: {concept.Group.GetInstance("Student").GetAttributeValue("firstName")}\t\t\t");

            foreach (var attribute in concept.Attributes.Values)
            {
                if (attribute.Value is Double v)
                {
                    Console.Write($"{attribute.Definition.Name}: {v.ToString("00")}\t\t\t");
                }
                else if (attribute.Value is null)
                {
                    Console.Write($"{attribute.Definition.Name}: null\t\t\t");
                }
                else
                {
                    Console.Write($"{attribute.Definition.Name}: {attribute.Value.ToString()}\t\t\t");
                }
            }
            Console.WriteLine();
        }
    }
    private static string _connectionString = "data source=127.0.0.1;initial catalog=Mephi.Cybernetics.Omelea.ReportsConstructor.DbScripts;User Id=sa;Password=Password0#;Encrypt=False";

    private static OmeleaReportsContext CreateContext() => new OmeleaReportsContext(new DbContextOptionsBuilder<OmeleaReportsContext>().UseSqlServer(_connectionString).Options);

    private static async Task LoadedMarksRecalcDemo()
    {
        var repo = new ApprListRepository(CreateContext(), new ExpressionCompiler());

        //Загружаем оценки для ведомости TotalDoubled
        var conceptManager = await repo.LoadApprListDefsAsync();
        await repo.LoadStudentsAsync(conceptManager);
        var SumDoubled = await repo.LoadMarksAsync(conceptManager.GetConceptDef("SumDoubled"));

        //Выводим
        PrintConcepts(SumDoubled);

        //Меняем значение для Марисы
        conceptManager.InstanceGroups
            .Single(g => (string)g.GetInstance("Student").GetAttributeValue("firstName") == "Marisa")
            .GetInstance(SumDoubled)
                .SetAttribute("addition", 10);
        
        //Видим что оценка пересчиталась (локально) (были подтянуты нужны для этой ведомости зависимости, если определение вычислимых столбцов изменится нужно загрузить оценки заново)
        PrintConcepts(SumDoubled);
    }

    private static async Task MarksSaveLoadDemo()
    {
        var repo = new ApprListRepository(CreateContext(), new ExpressionCompiler());

        //Получаем определения ведомостей без наполнения
        var conceptManager = await repo.LoadApprListDefsAsync();
        await repo.LoadStudentsAsync(conceptManager);

        var Lab1 = conceptManager.GetConceptDef("Lab1");
        var Lab2 = conceptManager.GetConceptDef("Lab2");
        var Total = conceptManager.GetConceptDef("Total");
        var Student = conceptManager.GetConceptDef("Student");
        var SumDoubled = conceptManager.GetConceptDef("SumDoubled");

        //Заполняем оценки        
        var marisaGroup = conceptManager.InstanceGroups
            .Single(g => (string)g.GetInstance("Student").GetAttributeValue("firstName") == "Marisa")
            .CreateInstance(Lab1)
                .SetAttribute("testsScore", 15)
                .SetAttribute("theoryScore", 15)
            .End()
            .CreateInstance(Lab2)
                .SetAttribute("codeQualityScore", 10)
                .SetAttribute("numberOfFeatures", 3)
            .End()
            .CreateInstance(Total)
            .End()
            .CreateInstance(SumDoubled)
                .SetAttribute("addition", 0)
            .End();
            

        var cirnoGroup = conceptManager.InstanceGroups
            .Single(g => (string)g.GetInstance("Student").GetAttributeValue("firstName") == "Cirno")
            .CreateInstance(Lab1)
                .SetAttribute("testsScore", 9)
                .SetAttribute("theoryScore", 9)
            .End()
            .CreateInstance(Lab2)
                .SetAttribute("codeQualityScore", 9)
                .SetAttribute("numberOfFeatures", 2)
            .End()
            .CreateInstance(Total)
            .End()
            .CreateInstance(SumDoubled)
                .SetAttribute("addition", 0)
            .End();


        //Пушим заполненные оценки в БД
        foreach (var def in conceptManager.ConceptDefs.Values.Where(def => def.Name != "Student"))
        {
            await repo.SaveMarksAsync(def);
        }

        //Получаем их обратно, видим что всё в порядке
        var conceptManager2 = await repo.LoadApprListDefsAsync();
        await repo.LoadStudentsAsync(conceptManager2);
        foreach (var def in conceptManager2.ConceptDefs.Values.Where(def => def.Name != "Student"))
        {
            var apprListDef = await repo.LoadMarksAsync(def);
            PrintConcepts(apprListDef);
        }
    }

    private static async Task NotLoadedMarksRecalcDemo()
    {
        var repo = new ApprListRepository(CreateContext(), new ExpressionCompiler());

        //Загружаем оценки всех студентов из ведомости Lab1 
        var conceptManager = await repo.LoadApprListDefsAsync();
        await repo.LoadStudentsAsync(conceptManager);
        await repo.LoadMarksAsync(conceptManager.GetConceptDef("Lab1"));

        //Меняем значения в Lab1, сохраняем
        conceptManager.InstanceGroups
            .Single(g => (string)g.GetInstance("Student").GetAttribute("firstName").Value == "Cirno")
            .GetOrCreateInstance("Lab1")
                .SetAttribute("testsScore", 15)
                .SetAttribute("theoryScore", 15);
        await repo.SaveMarksAsync(conceptManager.GetConceptDef("Lab1"));

        //Загружаем ведомости в другой менеджер концептов, видим, что значения для Сырно поменялись даже в таблицах, которые не были загружены
        // (значения пересчитались в бд)
        var conceptManager2 = await repo.LoadApprListDefsAsync();
        await repo.LoadStudentsAsync(conceptManager2);

        await repo.LoadMarksAsync(conceptManager2.GetConceptDef("Lab1"));
        PrintConcepts(conceptManager2.GetConceptDef("Lab1"));

        await repo.LoadMarksAsync(conceptManager2.GetConceptDef("Total"));
        PrintConcepts(conceptManager2.GetConceptDef("Total"));

        await repo.LoadMarksAsync(conceptManager2.GetConceptDef("SumDoubled"));
        PrintConcepts(conceptManager2.GetConceptDef("SumDoubled"));
    }

    private static async Task AppraisalListDefenitionsSaveLoadDemo()
    {
        //Parsing example.txt
        var compiler = new ConceptMappingDSLCompiler();
        var conceptDtoConverter = new ConceptDtoConverter();
        var conceptManager = compiler.Compile(File.ReadAllText("example.txt"));

        //setting up repository
        using var repository = new ApprListRepository(CreateContext(), new ExpressionCompiler());

        // saving definitions to db
        await repository.SaveDefsAsync(conceptManager);

        //loading saved defs and writing to db-output.json
        var conceptManager2 = await repository.LoadApprListDefsAsync();
        var apprListDefsDto = conceptDtoConverter.DefinitionsToDto(conceptManager2);
        File.WriteAllText("db-output.json", JsonSerializer.Serialize(apprListDefsDto, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task ClearDb()
    {
        using var repository = new ApprListRepository(CreateContext(), new ExpressionCompiler());
        await repository.SaveDefsAsync(new ConceptManager());
    }

    static async Task Main(string[] args)
    {
        // await ClearDb();
        // await AppraisalListDefenitionsSaveLoadDemo();
        Console.WriteLine(nameof(MarksSaveLoadDemo));
        await MarksSaveLoadDemo();
        // Console.WriteLine("");
        // Console.WriteLine(nameof(NotLoadedMarksRecalcDemo));
        // await NotLoadedMarksRecalcDemo();
        // Console.WriteLine("");
        // Console.WriteLine(nameof(LoadedMarksRecalcDemo));
        // await LoadedMarksRecalcDemo();

        // var optionsBuilder = new DbContextOptionsBuilder<OmeleaReportsContext>()
        //     .UseSqlServer(_connectionString)
        //     .EnableSensitiveDataLogging(true);
        // using var repo = new ApprListRepository(optionsBuilder.Options, new ExpressionCompiler());

        // var conceptManager = await repo.LoadApprListDefsAsync();
        // await repo.LoadStudentsAsync(conceptManager);
        // var Exam = await repo.LoadMarksAsync(conceptManager.GetConceptDef("Exam"));
        // var Exam2 = await repo.LoadMarksAsync(conceptManager.GetConceptDef("Exam2"));
        // PrintConcepts(Exam);
        // PrintConcepts(Exam2);
    }
}

using Mephi.Cybernetics.Race.ReportsDescriptionLanguage;

static class Program
{
    static void PrintConcepts(params ConceptInstance[] concepts)
    {
        foreach (var concept in concepts)
        {
            Console.Write($"name: {concept.Group.GetInstance("Student").GetAttributeValue("name")}\t\t");

            foreach (var attribute in concept.Attributes.Values)
            {
                if (attribute.Value is Double v)
                {
                    Console.Write($"{attribute.Definition.Name}: {v.ToString("00")}\t\t");
                }
                else
                {
                    Console.Write($"{attribute.Definition.Name}: {attribute.Value.ToString()}\t\t");
                }
            }
            Console.WriteLine();
        }
    }

    static void Main(string[] args)
    {
        var exprCompiler = new ExpressionCompiler();
        var compiler = new ConceptMappingDSLCompiler();
        var conceptManager = new ConceptManager();

        var Student = conceptManager.DefineConcept("Student")
            .AddAttribute("name", typeof(string));

        conceptManager = compiler.Compile(conceptManager, File.ReadAllText("example.txt"));
        var Lab1 = conceptManager.GetConceptDef("Lab1");
        var Lab2 = conceptManager.GetConceptDef("Lab2");
        var Total = conceptManager.GetConceptDef("Total");
        

        //пример задания вычислимых атрибутов ручками
        var Avg = conceptManager.DefineConcept("Avg")
            .AddCompAttribute("lab1Score", typeof(string),
            exprCompiler.Compile(conceptManager.GetConceptDef("Avg").GetContext(), "Lab1.score"))
            .AddCompAttribute("lab2Score", typeof(string),
            exprCompiler.Compile(conceptManager.GetConceptDef("Avg").GetContext(), "Lab2.score"))
            .AddCompAttribute("avgScore", typeof(string),
            exprCompiler.Compile(conceptManager.GetConceptDef("Avg").GetContext(), "(Lab1.score + Lab2.score) / 2"));

        var marisaGroup = conceptManager.CreateInstanceGroup()
            .CreateInstance(Student)
                .SetAttribute("name", "Marisa")
            .End()
            .CreateInstance(Lab1)
                .SetAttribute("testsScore", 15)
                .SetAttribute("theoryScore", 15)
            .End()
            .CreateInstance(Lab2)
                .SetAttribute("codeQualityScore", 10)
                .SetAttribute("numberOfFeatures", 3)
            .End()
            .CreateInstance(Avg)
            .End()
            .CreateInstance(Total)
            .End();
            

        var cirnoGroup = conceptManager.CreateInstanceGroup()
            .CreateInstance(Student)
                .SetAttribute("name", "Cirno")
            .End()
            .CreateInstance(Lab1)
                .SetAttribute("testsScore", 9)
                .SetAttribute("theoryScore", 9)
            .End()
            .CreateInstance(Lab2)
                .SetAttribute("codeQualityScore", 9)
                .SetAttribute("numberOfFeatures", 2)
            .End()
            .CreateInstance(Avg)
            .End()
            .CreateInstance(Total)
            .End();


        Console.WriteLine("Lab1:");
        PrintConcepts(marisaGroup.GetInstance(Lab1), cirnoGroup.GetInstance(Lab1));

        Console.WriteLine("Lab2:");
        PrintConcepts(marisaGroup.GetInstance(Lab2), cirnoGroup.GetInstance(Lab2));

        Console.WriteLine("Total:");
        PrintConcepts(marisaGroup.GetInstance(Total), cirnoGroup.GetInstance(Total));

        Console.WriteLine("Avg:");
        PrintConcepts(marisaGroup.GetInstance(Avg), cirnoGroup.GetInstance(Avg));

        Console.WriteLine("Test:");
        PrintConcepts(marisaGroup.GetOrCreateInstance("Test"), cirnoGroup.GetOrCreateInstance("Test"));
    }
}

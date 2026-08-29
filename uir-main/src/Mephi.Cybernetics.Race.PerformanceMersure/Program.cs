using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Mephi.Cybernetics.Race.Core.DomainModelMapping;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using System.Drawing;
using Mephi.Cybernetics.Race.Core.ObjectMapping;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;

namespace Mephi.Cybernetics.Race.PerformanceMersure
{
    public class MyBenchmark
    {
        public Ellipse ellipse;

        private MappingPoco<Ellipse, Ellipse2> newMapper;
        private Mapping<Ellipse, Ellipse2> oldMapper;

        [Params(100, 1000, 10000)] // Example: Varying input sizes
        public int DataSize;


        [GlobalSetup]
        public void Setup()
        {
            // Call your prepare function here
            newMapper = MappingPoco<Ellipse, Ellipse2>.New().UsingCustomInstantiation(o => new Ellipse2());
            newMapper.Property("Location").UsingMap<Ellipse, Point>(source => new Point(source.X, source.Y));
            newMapper.Property("Semiaxises").UsingMap<Ellipse, Size>(source => new Size(source.LongSemiAxis, source.ShortSemiAxis));

            oldMapper = new Mapping<Ellipse, Ellipse2>();
            oldMapper.UsingCustomInstantiation(o => new Ellipse2());
            oldMapper.TargetProperties["Label"].FromProperty<string, string>("Label", Mapping<string>.IdentityMapping);
            oldMapper.TargetProperties["Location"].UsingMap(source => new Point(source.X, source.Y));
            oldMapper.TargetProperties["Semiaxises"].UsingMap(source => new Size(source.LongSemiAxis, source.ShortSemiAxis));

            ellipse = new Ellipse
            {
                Label = "111",
                LongSemiAxis = 4,
                ShortSemiAxis = 3,
                X = 5,
                Y = -6
            };
        }

        [Benchmark]
        public void NewMapper()
        {
            for(int i = 0; i < DataSize; i++)
            {
                newMapper.Map(ellipse);
            }
        }

        [Benchmark]
        public void OldMapper()
        {
            for (int i = 0; i < DataSize; i++)
            {
                oldMapper.Convert(ellipse);
            }
        }
    }



    class Program
    {
        static void Main(string[] args)
        {

            var summary = BenchmarkRunner.Run<MyBenchmark>(
            // Add the MemoryDiagnoser exporter
            DefaultConfig.Instance.With(MemoryDiagnoser.Default)
                                    .With(MarkdownExporter.Default) // Optionally add other exporters
        );
        }
    }
}




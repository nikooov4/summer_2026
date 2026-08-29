using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests
{
    [TestFixture]
    public class AsyncProcessTestsSta
    {
        [Test]
        public void TestAsyncProcessCreate()
        {
            var source = new List<object>{ 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            Func<List<int>, IEnumerable<int>> getOdd = (list) => list.Where(i => i % 2 != 0).ToList();
            Func<List<int>, IEnumerable<int>> getEven = (list) => list.Where(i => i % 2 == 0).ToList();
            Func<IEnumerable<int>, IEnumerable<int>> inc = (list) => list.Select(i => i + 1);

            var simpleProcess1 = new AsyncProcessInstance(getEven);
            var simpleProcess2 = new AsyncProcessInstance(getOdd);

            var engine = new StaAsyncEngine();
            engine.Value(source);


            engine.Then(new PairingAsyncProcess(simpleProcess1, simpleProcess2)) 
                //.Then(AsyncProcess.Map(AsyncProcess.FromDelegate( (Tuple<IEnumerable<int>, IEnumerable<int>> arg) => arg))
                .Then(AsyncProcessInstance.FromDelegate((Tuple<List<object>, List<object>> arg) =>
                        {
                            var length = System.Math.Min(arg.Item1.Count(), arg.Item1.Count());
                            var res = new List<Tuple<object, object>>();
                            for (var index = 0; index < length; index++)
                                res.Add(new Tuple<object, object>(arg.Item1.ElementAt(index), arg.Item1.ElementAt(index)));
                            return res;
                        }))

                .Then(AsyncProcessInstance.FromDelegate((List<Tuple<object, object>> list) => list.Select(i => (int)i.Item1 + (int)i.Item2)));
            //var asyncEngine = new MtAsyncEngine();
            //var result = asyncEngine.Call(source);
            var result = engine.Run();
            Console.WriteLine(result);
            //Assert.AreEqual(expectedValue, result);
        }
    }
}

using System;
using Mephi.Cybernetics.Race.Core.Attributes.ComplexObjects;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess
{
    public class PairingAsyncProcess : IAsyncProcess
    {
        private readonly IAsyncProcess _first;
        private readonly IAsyncProcess _second;

        public PairingAsyncProcess(IAsyncProcess first, IAsyncProcess second)
        {
            _first = first;
            _second = second;
            State = ProcessState.Idle;
        }

        [SubObject]
        public IAsyncProcess First { get { return _first; } }
        [SubObject]
        public IAsyncProcess Second { get { return _second; } }

        public object Run()
        {
            var result1 = _first.Run();
            var result2 = _second.Run();

            Result = new Tuple<object, object>(result1, result2);
            State = ProcessState.Finished;
            return Result;
        }

        public object Result { get; set; }
        public Delegate Function { get; set; }
        public object Argument { get; set; }
        public ProcessState State { get; set; }
        public int Id { get; set; }
    }
}

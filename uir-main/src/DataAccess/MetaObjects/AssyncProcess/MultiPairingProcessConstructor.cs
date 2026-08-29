using System;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess
{
    public class MultiPairingProcessConstructor : IAsyncProcess
    {
        private readonly IAsyncProcess[] _processes;


        public MultiPairingProcessConstructor(params IAsyncProcess[] processes)
        {
            this._processes = processes;
            State = ProcessState.Idle;
        }

        public object Run()
        {
            var result = new List<object>();
            foreach (var pr in _processes)
            {
                result.Add(pr.Run());
            }

            Result = result;
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
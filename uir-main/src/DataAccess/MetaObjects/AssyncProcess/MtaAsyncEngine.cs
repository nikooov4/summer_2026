using System.Collections.Generic;
using System.Threading;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess
{
    public class MtaAsyncEngine
    {
        private readonly List<object> _result;
        private readonly TaskPriorityQueue _queue;
        private int _id = 0;

        public MtaAsyncEngine()
        {
            _queue = new TaskPriorityQueue();
            _result = new List<object>();
        }

        private object FunctionArg { get; set; }


        public MtaAsyncEngine Then(IAsyncProcess process)
        {
            _queue.AddItems(process);
            _queue.Priority++;
            _id++;
            process.Id = _id;
            return this;
        }

        public MtaAsyncEngine Add(IAsyncProcess process)
        {
            _queue.AddItem(process, _queue.Priority);
            _id++;
            process.Id = _id;
            return this;
        }

        public bool Run()
        {
            var pool = new ThreadQueue(_queue, 4);
            var thread = new Thread(pool.Start);
            thread.Start();
            return true;
        }

        public void Value(object arg)
        {
            FunctionArg = arg;
        }

    }

}
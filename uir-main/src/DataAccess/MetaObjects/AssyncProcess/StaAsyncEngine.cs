using System.Collections.Generic;
using System.Threading;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess
{
    public class StaAsyncEngine
    {
        private readonly List<object> _result;
        private readonly TaskPriorityQueue _queue;
        private int _id = 0;
        private Thread _workingThread;

        public StaAsyncEngine()
        {
            _queue = new TaskPriorityQueue();
            _result = new List<object>();
        }

        private object FunctionArg { get; set; }


        public StaAsyncEngine Then(IAsyncProcess process)
        {
            _queue.AddItems(process);
            _queue.Priority++;
            _id++;
            process.Id = _id;
            return this;
        }

        public StaAsyncEngine Add(IAsyncProcess process)
        {
            _queue.AddItem(process, _queue.Priority);
            _id++;
            process.Id = _id;
            return this;
        }

        //TODO: пересмотреть реализацию метода; объединить с MtaAsyncEngine
        public bool Run()
        {
            var pool = new ThreadQueue(_queue, 4);
//            Task proc = Task.Run(() => {
            foreach (var task in _queue)
            {
                foreach (var process in task)
                {
                    process.Argument = FunctionArg;
                    _result.Add(process.Run());
                }
                FunctionArg = _result.ToArray();
                _result.Clear();
            }
//            });
            return true;
        }

        public void Value(object arg)
        {
            FunctionArg = arg;
        }

    }

}
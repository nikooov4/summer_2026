using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess
{
    public class TaskPriorityQueue : IEnumerable<List<IAsyncProcess>>, IEnumerator<List<IAsyncProcess>>
    {
        private List<List<IAsyncProcess>> _taskQueue;
        private int _position = -1;

        public TaskPriorityQueue()
        {
            _taskQueue = new List<List<IAsyncProcess>>();
            Priority = 0;
        }

        public void AddItems(params IAsyncProcess[] processes)
        {
            _taskQueue.Add(new List<IAsyncProcess>(processes));
        }

        public void AddItem(IAsyncProcess process, int id = 0)
        {
            _taskQueue[id].Add(process);
        }



        public IAsyncProcess GetProcById(int id)                //Подумать над тем, что получаю процесс Instance
        {
            IAsyncProcess api = new AsyncProcessInstance();
            foreach (var queue in _taskQueue)
            {
                foreach (var proc in queue)
                {
                    if (proc.Id == id)
                        api = proc;
                }
            }
            return api;
        }

        public int GetPriorById(int id)
        {
            int result = 0;
            for (var i = 0; i < _taskQueue.Count; i++)
            {
                foreach (var proc in _taskQueue[i])
                {
                    if (proc.Id == id)
                        result = i;
                }
            }
            return result;
        }

        public IEnumerator<List<IAsyncProcess>> GetEnumerator()
        {
            var pq = new TaskPriorityQueue();
            return pq.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<IAsyncProcess> Current
        {
            get { return _taskQueue[_position]; }
        }

        public bool MoveNext()
        {
            _position++;
            return (_position < _taskQueue.Count);
        }

        public void Reset()
        {
            _position = 0;
        }

        public void Dispose()
        {
            _taskQueue = null; //Если ошибка, обратить внимание на этот метод. Не знаю, откуда вызывается, но требуется интерфейсом.
        }

        object System.Collections.IEnumerator.Current
        {
            get { return _taskQueue[_position]; }
        }

        public bool CanGetNext()
        {
            foreach (var task in _taskQueue[_position])
            {
                if (task.State == ProcessState.Idle)
                    return true;
            }
            foreach (var task in _taskQueue[_position])
            {
                if (task.State == ProcessState.Running)
                    return false;
            }
            _position++;
            return true;
        }


        public IAsyncProcess GetNext()
        {
            foreach (var task in _taskQueue[_position])
            {
                if (task.State == ProcessState.Idle)
                    return task;
            }
            return null;
        }

        public int Priority { get; set; }
    }

}
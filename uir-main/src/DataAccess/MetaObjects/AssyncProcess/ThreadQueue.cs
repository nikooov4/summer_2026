using System.Collections.Generic;
using System.Threading;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess
{

    public class ThreadQueue
    {
        private readonly List<Thread> _threadQueue = new List<Thread>();
        private readonly TaskPriorityQueue _queue;
        private readonly int _threadCount;
        private bool _isStopping = false;

        public ThreadQueue(TaskPriorityQueue queue, int threadCount)
        {
            _queue = queue;
            _threadCount = threadCount;
            for (var i = 0; i < threadCount; i++)
            {
                _threadQueue.Add(new Thread(() =>
                {
                    while (true)
                    {
                        if (_queue.CanGetNext())
                        {
                            _queue.GetNext().Run();
                        }
                        if (_isStopping)
                            break;
                        Thread.Sleep(5);
                    }
                }));
            }
        }

        public int ThreadCount
        {
            get { return _threadCount; }
        }


        public void Start()
        {
            foreach (var thread in _threadQueue)
            {
                thread.Start();
            }
        }

        /// <summary>
        /// Stops all working threads, their current work is allowed to be completed.
        /// </summary>
        public void Stop()
        {
            _isStopping = true;
        }

        /// <summary>
        /// Invokes immediate abort on all working threads. 
        /// </summary>
        public void Abort()
        {
            foreach (var thread in _threadQueue)
            {
                thread.Abort();
            }
        }
    }

}


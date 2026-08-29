using System;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess
{

public class CompositionProcess : IAsyncProcess
    {
    private readonly IAsyncProcess[] processes;

    public CompositionProcess(params IAsyncProcess[] processes)
        {
            Array.Reverse(processes);
            this.processes = processes;
        }

        public object Run()
        {
            foreach (var pr in processes)
            {
                pr.Argument = pr.Run();
            }

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
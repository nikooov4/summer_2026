using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess
{

    public enum ProcessEventType
    {
        Finish,
        Start,
        Error
    }

    //public class AsyncProcessInstance<TDomain> : IAsyncProcess<TDomain>
    public class AsyncProcessInstance : IAsyncProcess//<TDomain>
    {
        public AsyncProcessInstance(Delegate function, Object[] args)
        {
            State = ProcessState.Idle;
            Function = function;
            Argument = args;
            Result = null;
            Id++;
        }

        public AsyncProcessInstance(Delegate function)
        {
            State = ProcessState.Idle;
            Function = function;
            Argument = null;
            Result = new List<object>();
            Id++;
        }

        public AsyncProcessInstance()
        {

        }

        public static AsyncProcessInstance FromDelegate<TArg, TResult>(PropertyDescription.Func<TArg, TResult> function)
        {
            return new AsyncProcessInstance(function);
        }

        public object Run()
        {
            State = ProcessState.Running;
            Result = Function.DynamicInvoke(Argument);
            State = ProcessState.Finished;

            return Result;
        }

        public object Map(Delegate function)
        {
            if (Result != null)
            {
                State = ProcessState.Running;
                Result = Function.DynamicInvoke(Result);
                State = ProcessState.Finished;
            }
            return Result;
        }

        public object Reduce()
        {

            return this;
        }

        public object Result { get; set; }
        public Delegate Function { get; set; }
        public object Argument { get; set; }
        public ProcessState State { get; set; }
        public int Id { get; set; }
    }


}
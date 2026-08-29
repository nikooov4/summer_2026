using System;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.AssyncProcess 
{
    //TODO: Проверить правильность проектирования
   public interface IAsyncProcess//<out TDomain> : IProcess<TDomain>
   {
        object Run();
        object Result { get; set; }         // Почему setter должен быть общедоступен?
        Delegate Function { get; set; }     // Почему setter должен быть общедоступен?
        object Argument { get; set; }       // Почему setter должен быть общедоступен?
        ProcessState State { get; set; }    // Почему setter должен быть общедоступен?
        int Id { get; set; }                // Почему setter должен быть общедоступен?
    }

   public enum ProcessState
   {
       Idle,
       Running,
       Finished
   }

}
using System;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core
{
    public class ProcessDescription
    {
        private static readonly Dictionary<Type, ProcessDescription> _byType = new Dictionary<Type, ProcessDescription>();
        private static readonly Dictionary<ObjectDescription, ProcessDescription> _byObjectDescription = new Dictionary<ObjectDescription, ProcessDescription>();

        //public static readonly ProcessDescription 

        public static bool IsProcess(Type type)
        {
            throw new NotImplementedException();
        }

        public static bool IsProcess(ObjectDescription objectDescription)
        {
            throw new NotImplementedException();
        }

        public static ProcessDescription GetProcessDescription(Type type)
        {
            lock (_byType)
                if (!_byType.ContainsKey(type))
                {
                    var objectDescription = ObjectDescription.Get(type);
                    var processDescription = objectDescription.IsComplex ? new ProcessConstructorDescription(objectDescription) : new ProcessDescription(objectDescription);
                    _byType.Add(type, processDescription);
                    _byObjectDescription.Add(objectDescription, processDescription);
                    processDescription.Init();
                }
            return _byType[type];
        }

        public static ProcessDescription GetProcessDescription<T>()
        {
            return GetProcessDescription(typeof(T));
        }

        public static ProcessDescription GetProcessDescription(ObjectDescription objectDescription)
        {
            lock (_byType)
                if (!_byObjectDescription.ContainsKey(objectDescription))
                {
                    var entityDescription = objectDescription.IsComplex ? new ProcessConstructorDescription(objectDescription) : new ProcessDescription(objectDescription);
                    _byObjectDescription.Add(objectDescription, entityDescription);
                    _byType.Add(objectDescription.ClrType, entityDescription);
                    entityDescription.Init();
                }
            return _byObjectDescription[objectDescription];
        }


        private readonly ObjectDescription _objectDescription;


        internal ProcessDescription(ObjectDescription objectDescription)
        {
            _objectDescription = objectDescription;
            var t = typeof(PropertyDescription.Func<int, string>);
            
            var d = null as Delegate;
            //d.Method.GetParameters()[0].
        }

        protected internal virtual void Init()
        {
        }

        public object Call(object[] args)
        {
            throw new NotImplementedException();
        }

        public TResult Call<TArg, TResult>(TArg arg)
        {
            throw new NotImplementedException();
        }

        public TResult Call<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2)
        {
            throw new NotImplementedException();
        }

        public TResult Call<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            throw new NotImplementedException();
        }
    }

    public class ProcessConstructorDescription : ProcessDescription
    {
        internal ProcessConstructorDescription(ObjectDescription objectDescription) : base(objectDescription)
        {
            
        }

        protected internal override void Init()
        {
            base.Init();
        }
    }

    public class SubProcessInfo : SubObjectInfoBase<ProcessDescription>
    {
        public SubProcessInfo(ProcessDescription subObject, string role, int index)
            : base(subObject, role, index)
        {
        }

        public override object GetSubObject(object o)
        {
            throw new NotImplementedException();
        }
    }

    public class SubProcessInfoCollection : SubObjectInfoCollectionBase<SubProcessInfo, ProcessDescription>
    {
        internal SubProcessInfoCollection(IEnumerable<SubProcessInfo> objectInfos) : base(objectInfos)
        {
        }
    }
}

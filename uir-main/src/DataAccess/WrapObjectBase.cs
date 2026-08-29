using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public abstract class WrapObjectBase: IWrapObject
	{
		protected object _ID;
		private WrapObjectState _state = WrapObjectState.Unchanged;

		public WrapObjectBase()
		{
		}

		[IdField]
		public object ID
		{
			get
			{
				return _ID;
			}
			set
			{
				_ID = value;
			}
		}

		public WrapObjectState State
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
			}
		}

        public EntityBase WrappedObject
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
    }
}

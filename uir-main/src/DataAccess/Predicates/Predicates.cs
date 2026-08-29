using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.Predicates
{
	public delegate bool NaryOperation(params object[] oo);
	public delegate bool BinaryOperation(object o1, object o2);
	public delegate bool UnaryOperation(object o);
	public delegate bool NaryOperation<T>(params T[] oo);
	public delegate bool BinaryOperation<T>(T o1, T o2);
	public delegate bool UnaryOperation<T>(T o);

	public static class Functions
	{
		public static bool And(params bool[] pars)
		{
			bool res = true;
			foreach ( bool b in pars )
				if ( !( b && res ) )
					return false;
			return true;
		}

		public static bool Or(params bool[] pars)
		{
			bool res = false;
			foreach ( bool b in pars )
				if (  b || res  )
					return true;
			return false;
		}

		public static bool Not(bool par)
		{
			return !par;
		}

		new public static bool Equals(object o1, object o2)
		{
			return o1 == o2;
		}

		//public static bool Equals<T>(T o1, T o2)
		//{
		//    return o1 == o2;
		//}

		public static bool NotEquals(object o1, object o2)
		{
			return o1 != o2;
		}

		//public static bool NotEquals<T>(T o1, T o2)
		//{
		//    return o1 != o2;
		//}

        public static List<T> Select<T>(IList<T> list, UnaryOperation<T> where)
		{
            List<T> l = new List<T>();
			foreach ( T e in list )
				if ( where(e) )
					l.Add(e);
			return l;
		}

		public static T SelectFirst<T>(IList<T> list, UnaryOperation<T> where)
		{
		    foreach ( var element in list )
		    {
                if (where(element))
                    return element;
		    }
		    return default(T);
		}
	}

	public static class PartialApplication
	{
		public static UnaryOperation Apply(BinaryOperation binOp, object arg)
		{
			return delegate(object e)
			{
				return binOp(arg, e);
			};
		}
	}
}

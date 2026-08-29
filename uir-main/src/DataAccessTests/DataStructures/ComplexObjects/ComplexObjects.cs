using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.Attributes.ComplexObjects;
using NUnit.Framework.Constraints;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataStructures.ComplexObjects
{
    [ObjectConstructor]
    public class BookConstructor
    {
        public string Name { get; set; }
        [SubObject]
        public Unit Preface { get; set; }
        [SubObject]
        public IList<Unit> Content { get; set; }
        [SubObject]
        public IList<Author> Authors { get; set; }
    }

    public class Author
    {
        public string Name { get; set; }
        public int Year { get; set; }

    }

    [ObjectConstructor]
    public class ComplexUnit : Unit
    {
        [SubObject]
        public IList<Unit> SubUnits { get; set; }
    }

    public class Unit
    {
        public string Name { get; set; }
        public string Text { get; set; }
    }

    public interface IFeature
    {
        bool Available { get; }
    }

    public interface IAdditionalFeature
    {
        bool Runnable { get; }
    }

    public class Doer : IFeature, IAdditionalFeature
    {
        private static readonly Doer _defaultDoer = new Doer(false, false);
        public static Doer DefaultDoer = _defaultDoer;
        
        private readonly bool _available;
        private readonly bool _runnable;

        public Doer(bool available, bool runnable)
        {
            _available = available;
            _runnable = runnable;
        }

        bool IFeature.Available
        {
            get
            {
                return _available;
            }
        }

        public virtual bool Runnable
        {
            get { return _runnable; }
            //set { _runnable = available; }
        }
    }

    public class Doer2 : Doer
    {
        public Doer2(bool available, bool runnable) : base(available, runnable)
        {
        }

        public bool Available
        {
            get
            {
                return (this as IFeature).Available;
            }
        }

        public override bool Runnable
        {
            get { return true; }
        }
    }

    public class Doer3 : Doer2
    {
        public Doer3(bool available, bool runnable) : base(available, runnable)
        {
        }
    }
}

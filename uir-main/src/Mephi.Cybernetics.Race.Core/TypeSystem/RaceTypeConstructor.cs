using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.TypeSystem
{
    public class RaceTypeConstructor
    {
        private string _typeName;
        private string _typeNamespace;
        private string _typeSource;
        private RaceTypeSubtypingContructor _raceTypeSubtypingContructor;
        private RaceFunctionTypeConstructor _raceFunctionTypeConstructor;

        internal RaceTypeConstructor()
        {

        }

        public RaceTypeConstructor Name(string name)
        {
            _typeName = name;
            return this;
        }

        public RaceTypeConstructor Namespace(string nameSpace)
        {
            _typeNamespace = nameSpace;
            return this;
        }

        public RaceTypeConstructor Source(string source)
        {
            _typeSource = source;
            return this;
        }

        public RaceTypeSubtypingContructor Subtyping()
        {
            return _raceTypeSubtypingContructor ?? (_raceTypeSubtypingContructor = new RaceTypeSubtypingContructor(this));
        }

        public RaceFunctionTypeConstructor Function()
        {
            return _raceFunctionTypeConstructor ?? (_raceFunctionTypeConstructor = new RaceFunctionTypeConstructor(this));
        }
    }

    public class RaceTypeSubtypingContructor
    {
        private RaceTypeConstructor _raceTypeConstructor;
        private List<RaceType> _types;
        private List<RaceTypeConstructor> _typeConstructors;

        internal RaceTypeSubtypingContructor(RaceTypeConstructor raceTypeConstructor)
        {
            _raceTypeConstructor = raceTypeConstructor;
            _types = new List<RaceType>();
            _typeConstructors = new List<RaceTypeConstructor>();
        }
        public RaceTypeSubtypingContructor Parent(RaceType type)
        { 
            _types.Add(type);
            return this;
        }

        public RaceTypeSubtypingContructor Parent(RaceTypeConstructor type)
        {
            _typeConstructors.Add(type);
            return this;
        }

        public RaceTypeSubtypingContructor Parents(IList<RaceType> types)
        {
            _types.AddRange(types);
            return this;
        }

        public RaceTypeSubtypingContructor Parents(IList<RaceTypeConstructor> types)
        {
            _typeConstructors.AddRange(types);
            return this;
        }

        public RaceTypeConstructor End()
        {
            return _raceTypeConstructor;
        }
    }

    public class RaceFunctionTypeConstructor
    {
        private RaceTypeConstructor _raceTypeConstructor;
        private List<(string name, int index, RaceTypeConstructor typeConstructor)> _args1;
        private List<(string name, int index, RaceType type)> _args2;
        private int argumentCounter = 0;

        internal RaceFunctionTypeConstructor(RaceTypeConstructor raceTypeConstructor)
        {
            _raceTypeConstructor = raceTypeConstructor;
        }

        public RaceFunctionTypeConstructor Argument(string name, RaceTypeConstructor type)
        {
            _args1.Add((name, argumentCounter++, type));
            return this;
        }

        public RaceFunctionTypeConstructor Argument(string name, RaceType type)
        {
            _args2.Add((name, argumentCounter++, type));
            return this;
        }

        public RaceFunctionTypeConstructor Result(RaceTypeConstructor type) // TODO: RaceTypeRef
        {
            return this;
        }

        public RaceTypeConstructor End()
        {
            return _raceTypeConstructor;
        }
    }
}
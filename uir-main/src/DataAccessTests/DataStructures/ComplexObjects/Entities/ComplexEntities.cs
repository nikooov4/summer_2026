using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.Attributes.ComplexObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataStructures.ComplexObjects.Entities
{
    [ObjectConstructor]
    [EntityConstructor]
    [Entity]
    public class BookConstructor
    {
        public string Name { get; set; }
        [SubEntity]
        public Unit Preface { get; set; }
        [SubEntity]
        public IList<Unit> Content { get; set; }
        [SubEntity]
        public IList<Author> Authors { get; set; }
    }

    [Entity]
    public class Author
    {
        public static int Get(int i)
        {
            return 0;
        }

        public string Name { get; set; }
        public int Year { get; set; }

    }

    [EntityConstructor]
    public class ComplexUnit : Unit
    {
        [SubEntity]
        public IList<Unit> SubUnits { get; set; }
    }

    [Entity]
    public class Unit
    {
        public string Name { get; set; }
        public string Text { get; set; }
    }
}
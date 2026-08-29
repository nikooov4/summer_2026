using System;
using System.Collections.Generic;
using System.Linq;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.SemanticNet
{
    public abstract class RaceConcept : IDomain, IDomainSubtypingInfo
    {
        public const string AllConceptName = "All";
        public const string EmptyConceptName = "Empty";

        internal RaceConcept(string name, string ns, string source,
            IReadOnlyList<RaceConcept> parents)
        {
            Name = name;
            Namespace = ns;
            FullName = ns + "." + name;
            Source = source;
            FullyQualifiedName = $"[{Source}::{FullName}]";
            Parents = parents;
        }

        private static RaceConcept _All;
        private static RaceConcept _Empty;

        public static RaceConcept All => _All ??= new RaceSimpleConcept(AllConceptName, typeof(RaceConcept).Namespace,
            typeof(RaceConcept).Assembly.FullName, new List<RaceConcept>());

        public static RaceConcept Empty => _Empty ??= new RaceSimpleConcept(EmptyConceptName,
            typeof(RaceConcept).Namespace, typeof(RaceConcept).Assembly.FullName, new List<RaceConcept>());

        public static bool operator ==(RaceConcept c1, RaceConcept c2) =>
            c1?.FullyQualifiedName == c2?.FullyQualifiedName;

        public static bool operator !=(RaceConcept c1, RaceConcept c2) =>
            c1?.FullyQualifiedName != c2?.FullyQualifiedName;

        public static bool operator <(RaceConcept c1, RaceConcept c2) =>
            c1.IsSubtypeOf(c2) && c1 != c2;

        public static bool operator >(RaceConcept c1, RaceConcept c2) =>
            c2.IsSubtypeOf(c1) && c1 != c2;

        public static bool operator <=(RaceConcept c1, RaceConcept c2) =>
            c1 < c2 || c1 == c2;

        public static bool operator >=(RaceConcept c1, RaceConcept c2) =>
            c1 > c2 || c1 == c2;


        public static RaceConcept operator |(RaceConcept c1, RaceConcept c2)
        {
            return RaceComplexConcept.Union(c1, c2);
        }

        public static RaceConcept operator &(RaceConcept c1, RaceConcept c2)
        {
            return RaceComplexConcept.Intersect(c1, c2);
        }

        public static RaceConcept operator !(RaceConcept c)
        {
            return RaceComplexConcept.Complement(All, c);
        }

        public static RaceConcept operator -(RaceConcept c1, RaceConcept c2)
        {
            return RaceComplexConcept.Complement(c1, c2);
        }

        public static bool operator true(RaceConcept c) => c == All;
        public static bool operator false(RaceConcept c) => c == Empty;

        public string Name { get; }

        public string Namespace { get; }

        public string Source { get; }

        public string FullName { get; }

        public string FullyQualifiedName { get; }

        public Type UnderlyingType => typeof(RaceConcept);

        public IReadOnlyList<RaceConcept> Parents { get; }

        IReadOnlyList<IDomain> ISubtypingInfo<IDomain>.Parents => Parents;

        public bool IsSubtypeOf(IDomain domain) => domain.FullyQualifiedName == FullyQualifiedName ||
                                                   Parents.Any(concept =>
                                                       concept.FullyQualifiedName == domain.FullyQualifiedName);

        public abstract bool IsInstanceOf(object obj);
    }


    //TODO: реализовать
    // Варианты создания:
    // 1. На основе класса (все экз класса -- экз концепта)
    // 2. На основе enum
    // 3. Перечислением отдельных элементов данного класса
    // 4. Дизъюнктивная сумма (Either?)
    // 5. На основе простого предиката: C<T,T2>: T - тип экз, T2 - тип атрибута 
    public class RaceSimpleConcept : RaceConcept
    {
        public RaceSimpleConcept(string name, string ns, string source,
            IReadOnlyList<RaceConcept> parents) : base(name, ns, source, parents)
        {
        }

        public override bool IsInstanceOf(object obj)
        {
            throw new NotImplementedException();
        }
    }

    public class RaceConceptConstructor : IConstructor<IDomain>
    {
        private static RaceConceptConstructor _UnionConstructor;
        private static RaceConceptConstructor _IntersectionConstructor;
        private static RaceConceptConstructor _ComplementConstructor;

        public static RaceConceptConstructor UnionConstructor => _UnionConstructor
            ??= new RaceConceptConstructor(RaceConceptOperation.Union);

        public static RaceConceptConstructor IntersectionConstructor => _IntersectionConstructor
            ??= new RaceConceptConstructor(RaceConceptOperation.Intersection);

        public static RaceConceptConstructor ComplementConstructor => _ComplementConstructor
            ??= new RaceConceptConstructor(RaceConceptOperation.Complement);

        //TODO: а чего конструируем??? (объединение, пересечение..?)
        public static RaceConcept Construct(IEnumerable<RaceConcept> args) =>
            new RaceComplexConcept(args.FirstOrDefault()?.Name, args.FirstOrDefault()?.Namespace,
                args.FirstOrDefault()?.Source, args as IReadOnlyList<RaceConcept>);

        public static IDomain Construct(RaceConcept[] args) =>
            new RaceComplexConcept(args.FirstOrDefault()?.Name, args.FirstOrDefault()?.Namespace,
                args.FirstOrDefault()?.Source, args);

        private enum RaceConceptOperation
        {
            Union,
            Intersection,
            Complement
        }

        private RaceConceptConstructor(RaceConceptOperation operation)
        {
            _ = operation switch
            {
                RaceConceptOperation.Union => UnionConstructor,
                RaceConceptOperation.Intersection => IntersectionConstructor,
                RaceConceptOperation.Complement => ComplementConstructor,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };
        }


        public string Name { get; }

        public SignatureArgumentInfo<RaceConcept>[] Arguments => Arguments;

        public SignatureArgumentInfo<RaceConcept> this[int index] =>
            Arguments.FirstOrDefault(p => p.Index == index);

        public SignatureArgumentInfo<RaceConcept> this[string name] =>
            Arguments.FirstOrDefault(p => p.Name == name);


        IArgumentInfo<IDomain>[] ISignature<IDomain, IArgumentInfo<IDomain>>.Arguments =>
            Arguments;

        IArgumentInfo<IDomain> ISignature<IDomain, IArgumentInfo<IDomain>>.this[int index] =>
            Arguments.FirstOrDefault(p => p.Index == index);

        IArgumentInfo<IDomain> ISignature<IDomain, IArgumentInfo<IDomain>>.this[string name] =>
            Arguments.FirstOrDefault(p => p.Name == name);

        public int Arity { get; }


        IDomain IConstructor<IDomain>.Construct(IList<IDomain> args) => Construct((IEnumerable<RaceConcept>)args);


        IDomain IConstructor<IDomain>.Construct(IDomain[] args) => Construct((RaceConcept[])args);

        IDomain IFunctionSignature<IDomain, IArgumentInfo<IDomain>>.ReturnType => ReturnType;

        public RaceConcept ReturnType => throw new NotImplementedException();
    }

    public class RaceComplexConcept : RaceConcept, IStructureInfo<IDomain, RaceConceptConstructor>
    {
        private const string UnionKw = "UNION";
        public const string IntersectKw = "INTERSECT";
        public const string ExceptKw = "EXCEPT";

        public RaceComplexConcept(string name, string ns, string source, IReadOnlyList<RaceConcept> parents) :
            base(name, ns, source, parents)
        {
        }

        public static RaceComplexConcept Union(RaceConcept c1, RaceConcept c2)
        {
            var unionParents = new List<RaceConcept>();
            unionParents.AddRange(new List<RaceConcept> { c1, c2 });

            var unionName = $"{c1.FullyQualifiedName} {UnionKw} {c2.FullyQualifiedName}"; //string interpolation
            var unionNamespace = typeof(RaceComplexConcept).Namespace; //
            var unionSource = typeof(RaceComplexConcept).Assembly.FullName; //
            return new RaceComplexConcept(unionName, unionNamespace, unionSource, unionParents);
        }

        public static RaceComplexConcept Union(RaceConcept c1, RaceConcept c2, RaceConcept c3)
        {
            var unionParents = new List<RaceConcept>();
            unionParents.AddRange(new List<RaceConcept> { c1, c2, c3 });

            var unionName =
                $"{c1.FullyQualifiedName} {UnionKw} {c2.FullyQualifiedName} {UnionKw} {c3.FullyQualifiedName}"; //string interpolation
            var unionNamespace = typeof(RaceComplexConcept).Namespace; //
            var unionSource = typeof(RaceComplexConcept).Assembly.FullName; //
            return new RaceComplexConcept(unionName, unionNamespace, unionSource, unionParents);
        }

        public static RaceComplexConcept Union(RaceConcept[] cc)
        {
            var unionParents = new List<RaceConcept>();
            unionParents.AddRange(cc);
            var unionName = string.Join(UnionKw, cc.Select(p => p.Name));
            return new RaceComplexConcept(unionName, typeof(RaceComplexConcept).Namespace,
                typeof(RaceComplexConcept).Assembly.FullName, unionParents);
        }

        public static RaceComplexConcept Union(IEnumerable<RaceConcept> cc)
        {
            var unionParents = new List<RaceConcept>();
            var raceConcepts = cc.ToList();
            unionParents.AddRange(raceConcepts);
            var unionName = string.Join(UnionKw, raceConcepts.Select(p => p.Name));
            return new RaceComplexConcept(unionName, typeof(RaceComplexConcept).Namespace,
                typeof(RaceComplexConcept).Assembly.FullName, unionParents);
        }

        public static RaceComplexConcept Intersect(RaceConcept c1, RaceConcept c2) =>
            new(
                $"{c1.FullyQualifiedName} {IntersectKw} {c2.FullyQualifiedName}", typeof(RaceComplexConcept).Namespace,
                typeof(RaceComplexConcept).Assembly.FullName, new List<RaceConcept> { c1, c2 });


        public static RaceComplexConcept Intersect(RaceConcept c1, RaceConcept c2, RaceConcept c3) =>
            new(
                $"{c1.FullyQualifiedName} {IntersectKw} {c2.FullyQualifiedName} {IntersectKw} {c3.FullyQualifiedName}",
                typeof(RaceComplexConcept).Namespace,
                typeof(RaceComplexConcept).Assembly.FullName, new List<RaceConcept> { c1, c2, c3 });

        public static RaceComplexConcept Intersect(RaceConcept[] cc) =>
            new(string.Join(IntersectKw, cc.Select(p => p.Name)),
                typeof(RaceComplexConcept).Namespace,
                typeof(RaceComplexConcept).Assembly.FullName, cc);


        public static RaceComplexConcept Intersect(IEnumerable<RaceConcept> cc) =>
            new(string.Join(IntersectKw, cc.Select(p => p.Name)),
                typeof(RaceComplexConcept).Namespace,
                typeof(RaceComplexConcept).Assembly.FullName, cc.ToList());

        public static RaceComplexConcept Complement(RaceConcept c1, RaceConcept c2) =>
            new($"{c1.FullyQualifiedName} {ExceptKw} {c2.FullyQualifiedName}",
                typeof(RaceComplexConcept).Namespace,
                typeof(RaceComplexConcept).Assembly.FullName, new List<RaceConcept>(c1.Parents));


        public RaceConcept ReflectedDomain => throw new NotImplementedException();
        IDomain IStructureInfo<IDomain, RaceConceptConstructor>.ReflectedDomain => throw new NotImplementedException();

       //TODO: переделать на хранение непосредственно объекта-конструктора
        public RaceConceptConstructor Constructor => Name switch
        {
            _ when Name.Contains(UnionKw) => RaceConceptConstructor.UnionConstructor,
            _ when Name.Contains(IntersectKw) => RaceConceptConstructor.IntersectionConstructor,
            _ when Name.Contains(ExceptKw) => RaceConceptConstructor.ComplementConstructor,
            _ => throw new ArgumentOutOfRangeException()
        };

        public IList<RaceConcept> Components => Parents as IList<RaceConcept>;

        RaceConceptConstructor IStructureInfo<IDomain, RaceConceptConstructor>.Constructor =>
            Constructor;

        IList<IDomain> IStructureInfo<IDomain, RaceConceptConstructor>.Components =>
            Components as IList<IDomain>;

        public override bool IsInstanceOf(object obj)
        {
            throw new NotImplementedException();
        }
    }

    public class RaceRestrictionConcept : RaceComplexConcept
    {
        private Either<RaceFrame, Predicate<object>, Func<object, bool>> _conceptDefinition;

        public RaceRestrictionConcept(string name, string ns, string source, IReadOnlyList<RaceConcept> parents) :
            base(name, ns, source, parents)
        {
            Func<object, bool> predicate = null;
            Delegate.CreateDelegate(typeof(Predicate<object>),predicate.Method);
        }

        public static RaceRestrictionConcept FromFunc<T>(Func<T, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public static RaceRestrictionConcept FromPredicate<T>(Predicate<T> predicate)
        {
            throw new NotImplementedException();
        }

        public static RaceRestrictionConcept FromFFrame(RaceFrame frame)
        {
            throw new NotImplementedException();
        }


        public bool IsDefinedByFrame => throw new NotImplementedException();
        public bool IsDefinedByPredicate => throw new NotImplementedException();
    }
}
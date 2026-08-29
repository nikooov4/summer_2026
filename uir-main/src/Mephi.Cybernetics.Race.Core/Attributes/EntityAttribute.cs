using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.Core.Attributes
{
    public class IsEntityAttribute : Attribute
    {
        public IsEntityAttribute(string name = null, string ns = null, string source = null)
        {
            Name = name;
            Namespace = ns;
            Source = source;
        }

        public string Name { get; }
        public string Namespace { get; }
        public string Source { get; }
    }

    public class IsOneToOneAttribute : Attribute
    {
        public IsOneToOneAttribute(Cardinality sourceCardinality = null, Cardinality targetCardinality = null)
        {
            SourceCardinality = sourceCardinality ?? Cardinality.ZeroOrOne;
            TargetCardinality = targetCardinality ?? Cardinality.ZeroOrOne;
        }

        public Cardinality SourceCardinality { get; }
        public Cardinality TargetCardinality { get; }
    }

    public class IsOneToManyAttribute : Attribute
    {
        public IsOneToManyAttribute(string referecingPropertyName, bool isReferenceInDbOnly = false,
            Cardinality sourceCardinality = null, Cardinality targetCardinality = null)
        {
            ReferecingPropertyName = referecingPropertyName;
            IsReferenceInDbOnly = isReferenceInDbOnly;
            SourceCardinality = sourceCardinality ?? Cardinality.ZeroOrOne;
            TargetCardinality = targetCardinality ?? Cardinality.ZeroOrMore;
        }

        public string ReferecingPropertyName { get; }
        public bool IsReferenceInDbOnly { get; }
        public Cardinality SourceCardinality { get; }
        public Cardinality TargetCardinality { get; }
    }

    //TODO: добавить возможность задания нескольких для одного свойства, чтобы можно было дифференцировать биндинги для разных источников данных
    //[AttributeUsage(AttributeTargets.Property,true)]
    public class IsManyToManyAttribute : Attribute
    {
        public IsManyToManyAttribute(string connectingObjectNamespace, string connectingObjectName, string parentAttributeName, string childAttributeName)
        {
            ConnectingObjectNamespace = connectingObjectNamespace;
            ConnectingObjectName = connectingObjectName;
            ParentAttributeName = parentAttributeName;
            ChildAttributeName = childAttributeName;
        }

        public string ConnectingObjectNamespace { get; }
        public string ConnectingObjectName { get; }
        public string ParentAttributeName { get; }
        public string ChildAttributeName { get; }
    }

    public class IsRelationshipAttribute : Attribute
    {
        public IsRelationshipAttribute(string name = null, string ns = null, string source = null)
        {
            Name = name;
            Namespace = ns;
            Source = source;
        }

        public string Name { get; }
        public string Namespace { get; }
        public string Source { get; }
    }

    public class ParticipantCardinalityAttribute : Attribute
    {
        /// <summary>
        /// Задание кардинальности в формате [lower,higher]
        /// </summary>
        /// <param name="lower">нижняя граница кардинальности, должна быть неотрицательной</param>
        /// <param name="higher">верхняя граница кардинальности, должна быть не меньше lower</param>
        public ParticipantCardinalityAttribute(int lower, int higher)
        {
            Cardinality = Cardinality.ZeroOrOne.Minimal == lower && Cardinality.ZeroOrOne.Maximal == higher
                ? Cardinality.ZeroOrOne
                : Cardinality.ZeroOrMore.Minimal == lower && Cardinality.ZeroOrMore.Maximal == higher
                    ? Cardinality.ZeroOrMore
                    : Cardinality.One.Minimal == lower && Cardinality.One.Maximal == higher
                        ? Cardinality.One
                        : Cardinality.OneOrMore.Minimal == lower && Cardinality.OneOrMore.Maximal == higher
                            ? Cardinality.OneOrMore
                            : new Cardinality(lower, higher); // как-то реализовать создание подходящего Cardinality
        }

        public Cardinality Cardinality { get; }
    }
}
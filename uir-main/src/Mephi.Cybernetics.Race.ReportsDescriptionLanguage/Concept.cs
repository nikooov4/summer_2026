using System.Diagnostics;
using System.Collections.Generic;


//  TODO: Оповещать инстансы о добавлении и удалении атрибутов из дефениции
//  TODO: Возможность удаление дефениций концептов и атрибутов
//  TODO: Сделать определения и инстансы более независимыми от других определений и инстансов, чтобы можно было легче доставать данные из репозитория по частям, возможно ослабить условия проверок при которых выбрасваются исключения.
//  TODO: Ивент при изменении значений инстансов аттрибутов вместо прямого вызова метода (возможно также пригодится при оповещении UI слоя об изменениях)
//  TODO: Привести сообщения исключений к одному виду (возможно избавится от проверки недостижимых случаев)

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{

    public class ConceptManager
    {
        // public IExpressionCompiler ExpressionCompiler { get; }
        private Dictionary<string, ConceptDef> _conceptDefs;
        public IReadOnlyDictionary<string, ConceptDef> ConceptDefs => _conceptDefs;
        private List<ConceptInstanceGroup> _instanceGroups;
        public IReadOnlyList<ConceptInstanceGroup> InstanceGroups => _instanceGroups;
        public ConceptManager()
        {
            // ExpressionCompiler = expressionCompiler;
            _conceptDefs = new Dictionary<string, ConceptDef>();
            _instanceGroups = new List<ConceptInstanceGroup>();
        }

        public ConceptDef DefineConcept(string name)
        {
            var conceptDef = new ConceptDef(this, name);
            _conceptDefs.Add(name, conceptDef);
            return conceptDef;
        }

        public bool TryGetConceptDef(string conceptName, out ConceptDef concept)
        {
            return _conceptDefs.TryGetValue(conceptName, out concept);
        }

        public ConceptDef GetConceptDef(string conceptName)
        {
            if (TryGetConceptDef(conceptName, out ConceptDef ret))
            {
                return ret;
            }
            else
            {
                throw new KeyNotFoundException($"No such concept definition with name \"{conceptName}\"");
            }
        }

        public ConceptInstanceGroup CreateInstanceGroup()
        {
            var instanceGroup = new ConceptInstanceGroup(this);
            _instanceGroups.Add(instanceGroup);
            return instanceGroup;
        }

        public List<ExpressionCompilerArgInfo> GetContext()
        {
            return _conceptDefs.Values
            .SelectMany(
                x => x.Attributes.Values.Select(y => new ExpressionCompilerArgInfo(y.CSType, new List<string> { x.Name, y.Name }))
            ).ToList();
        }
    }

    public class ConceptInstanceGroup 
    {
        public ConceptManager ConceptManager { get; }
        private Dictionary<string, ConceptInstance> _instances;
        public IReadOnlyDictionary<string, ConceptInstance> Instances => _instances;

        public ConceptInstanceGroup(ConceptManager conceptManager)
        {
            ConceptManager = conceptManager;
            _instances = new Dictionary<string, ConceptInstance>();
        }

        public ConceptInstance CreateInstance(ConceptDef conceptDef)
        {
            var instance = new ConceptInstance(conceptDef, this);
            if (!_instances.TryAdd(conceptDef.Name, instance))
            {
                throw new InvalidOperationException($"Concept instance with definition \"{conceptDef.Name}\" already exists");
            }
            return instance;
        }

        public ConceptInstance CreateInstance(string conceptName)
        {
            return CreateInstance(ConceptManager.GetConceptDef(conceptName));
        }

        public ConceptInstance GetInstance(ConceptDef conceptDef)
        {
            if (!_instances.TryGetValue(conceptDef.Name, out var conceptInstance))
            {
                throw new InvalidOperationException($"Concept instance with definition \"{conceptDef.Name}\" doesn't exist");
            }
            return conceptInstance;
        }

        public ConceptInstance GetInstance(string conceptName)
        {
            return GetInstance(ConceptManager.GetConceptDef(conceptName));
        }
        
        public ConceptInstance GetOrCreateInstance(ConceptDef conceptDef)
        {
            if (_instances.TryGetValue(conceptDef.Name, out var instance))
            {
                return instance;
            }
            return CreateInstance(conceptDef);
        }

        public ConceptInstance GetOrCreateInstance(string conceptName)
        {
            return GetOrCreateInstance(ConceptManager.GetConceptDef(conceptName));
        }

        public bool TryGetInstance(ConceptDef conceptDef, out ConceptInstance? instance)
        {
            return _instances.TryGetValue(conceptDef.Name, out instance);
        }

        public bool TryGetInstance(string conceptName, out ConceptInstance? instance)
        {
            return TryGetInstance(ConceptManager.GetConceptDef(conceptName), out instance);
        }

        public ConceptInstance? GetInstanceOrNull(ConceptDef conceptDef)
        {
            _instances.TryGetValue(conceptDef.Name, out var instance);
            return instance;
        }

        public ConceptInstance? GetInstanceOrNull(string conceptName)
        {
            return GetInstanceOrNull(ConceptManager.GetConceptDef(conceptName));
        }

        public ConceptManager End()
        {
            return ConceptManager;
        }
    }

    public class ConceptDef
    {
        OrderedDictionary<string, AttributeDefBase> _attrs;
        public IReadOnlyDictionary<string, AttributeDefBase> Attributes => _attrs;
        public ConceptManager ConceptManager { get; }

        public List<ConceptInstance> GetInstances()
        {
            return ConceptManager.InstanceGroups
                .Select(g => g.GetOrCreateInstance(this.Name))
                .ToList();
        }

        public string Name { get; }

        public object this[string attrName]
        {
            get
            {
                return GetAttribute(attrName);
            }
        }

        internal ConceptDef(ConceptManager conceptsManager, string name)
        {
            Name = name;
            ConceptManager = conceptsManager;
            _attrs = new OrderedDictionary<string, AttributeDefBase>();
        }

        internal ConceptDef(ConceptManager conceptsManager, string name, IEnumerable<AttributeDefBase> attrs)
        {
            Name = name;
            ConceptManager = conceptsManager;
            _attrs = new OrderedDictionary<string, AttributeDefBase>(attrs.Select(x => KeyValuePair.Create(x.Name, x)));
        }

        public AttributeDefBase GetAttribute(string name)
        {
            if (!_attrs.ContainsKey(name))
            {
                throw new ArgumentException($"no such attribute \"{name}\"");
            }
            return _attrs[name];
        }

        public bool TryGetAttribute(string name, out AttributeDefBase? attr)
        {
            return _attrs.TryGetValue(name, out attr);
        }

        internal ConceptDef AddAttribute(AttributeDefBase attr)
        {
            if (_attrs.ContainsKey(attr.Name))
            {
                throw new ArgumentException($"Attribute with name {attr.Name} already exists");
            }
            AddOrUpdateAttribute(attr);
            return this;
        }

        //TODO: менять инстансы в соответсвии с определением при изменении
        internal ConceptDef AddOrUpdateAttribute(AttributeDefBase attr)
        {
            if (!_attrs.TryAdd(attr.Name, attr))
            {   
                var oldAttr = _attrs[attr.Name];
                _attrs[attr.Name] = attr;
                attr.SetOldAttrDef(oldAttr);
            }
            return this;
        }

        public ConceptDef UpdateCompAttribute(string name, Type type, ICompiledExpression expr)
        {
            AddOrUpdateAttribute(new ComputableAttributeDef(this, name, type, expr));
            return this;
        }

        public ConceptDef UpdateAttribute(string name, Type cstype)
        {
            AddOrUpdateAttribute(new AttributeDef(this, name, cstype));
            return this;
        }

        public ConceptDef AddCompAttribute(string name, Type type, ICompiledExpression expr)
        {
            AddAttribute(new ComputableAttributeDef(this, name, type, expr));
            return this;
        }

        public ConceptDef AddAttribute(string name, Type cstype)
        {
            AddAttribute(new AttributeDef(this, name, cstype));
            return this;
        }

        public List<ExpressionCompilerArgInfo> GetContext()
        {
            var context = ConceptManager.GetContext();
            context.AddRange
            (
                _attrs.Select(x => new ExpressionCompilerArgInfo(x.Value.CSType, new List<string> {x.Value.Name }))
            );
            return context;
        }

        public ConceptManager End()
        {
            return ConceptManager;
        }
    }

    // public interface IConceptInstance
    // {
    //     ConceptDef Definition { get; }
    //     // bool AreDependenciesLoaded { get; }
    //     ConceptInstance GetInstanceWithDeps();
    //     object? GetAttributeValue(string name);
    //     AttributeInstanceBase GetAttribute(string name);
    // }

    public class ConceptInstance
    {
        public ConceptDef Definition { get; }
        public ConceptInstanceGroup Group { get; }
        private OrderedDictionary<string, AttributeInstanceBase> _attributes;
        public IReadOnlyDictionary<string, AttributeInstanceBase> Attributes => _attributes;

        // public bool AreDependenciesLoaded => true;

        public ConceptInstance GetInstanceWithDeps()
        {
            return this;
        }

        internal ConceptInstance(ConceptDef definition, ConceptInstanceGroup group)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Group = group ?? throw new ArgumentNullException(nameof(group));
            _attributes = new OrderedDictionary<string, AttributeInstanceBase>();

            foreach (var attributeDef in Definition.Attributes.Values)
            {
                _attributes.Add(attributeDef.Name, attributeDef.CreateInstance(this));
            }
        }

        public ConceptInstance SetAttribute(string name, object? value)
        {
            if (!Definition.Attributes.ContainsKey(name))
            {
                throw new ArgumentException($"Attribute {name} is not defined in concept {Definition.Name}");
            }
            if (!_attributes.TryGetValue(name, out var attr))
            {
                throw new InvalidOperationException($"Attribute {name} is defined however isn't initialized for this instance (instance in invalid state?)");
            }
            attr.Value = value;
            return this;
        }

        //Использовать для инциализации из persistent storage в котором значения уже посчитаны (не инициирует пересчёт значений)
        public ConceptInstance SetAttributeCachedValue(string name, object? value)
        {
            if (!Definition.Attributes.ContainsKey(name))
            {
                throw new ArgumentException($"Attribute {name} is not defined in concept {Definition.Name}");
            }
            if (!_attributes.TryGetValue(name, out var attr))
            {
                throw new InvalidOperationException($"Attribute {name} is defined however isn't initialized for this instance (instance in invalid state?)");
            }
            attr.SetCahcedValue(value);
            return this;
        }

        public object? GetAttributeValue(string name)
        {
            return GetAttribute(name).Value;
        }

        public AttributeInstanceBase GetAttribute(string name)
        {
            bool isDefined = Definition.Attributes.ContainsKey(name);
            bool isInitialized = _attributes.ContainsKey(name);

            if (isDefined && !isInitialized)
            {
                throw new InvalidOperationException($"Attribute \"{name}\" is defined, but it wasn't initialized (instance in invalid state?)");
            }
            //когда нибудь я сделаю это нормально (
            if (!isDefined && isInitialized)
            {
                throw new ArgumentException($"Attribute {name} exists, however it was removed from definition (instance in invalid state?)");
            }
            if (!isDefined && !isInitialized)
            {
                throw new ArgumentException($"Attribute {name} not found in instance");
            }

            return _attributes[name];
        }

        public ConceptInstanceGroup End()
        {
            return Group;
        }
    }
    public record struct DependentAttrInfo(string ConceptName, string AttributeName);

    public abstract class AttributeDefBase
    {
        public abstract ConceptDef ParentConcept { get; }
        public abstract Type CSType { get; }
        public abstract string Name { get; }
        private List<DependentAttrInfo> _dependentAttrs = new List<DependentAttrInfo>();
        public IReadOnlyList<ComputableAttributeDef> DependentAttrs
        {
            get
            {
                return _dependentAttrs.Select
                (
                    x => (ComputableAttributeDef)ParentConcept
                        .ConceptManager
                        .GetConceptDef(x.ConceptName)
                        .GetAttribute(x.AttributeName)
                ).ToList();
            }
        }
        internal void SetOldAttrDef(AttributeDefBase oldAttrDef)
        {
            this._dependentAttrs = new List<DependentAttrInfo>(oldAttrDef._dependentAttrs);
        }
        internal abstract AttributeInstanceBase CreateInstance(ConceptInstance conceptInstance);
        // public bool IsAssignableFrom(Type type)
        // {
        //     return CSType.IsAssignableFrom(type);
        // }

        // public bool IsAssignableFrom(object? obj)
        // {
        //     return obj == null || IsAssignableFrom(obj.GetType());
        // }
        public List<AttributeInstanceBase> GetInstances()
        {
            return ParentConcept.GetInstances().Select(x => x.GetAttribute(Name)).ToList();
        }
        internal void AddDependentAttr(DependentAttrInfo dependentAttr)
        {
            _dependentAttrs.Add(dependentAttr);
        }
    }

    public class AttributeDef : AttributeDefBase
    {
        override public Type CSType { get; }
        override public string Name { get; }
        override public ConceptDef ParentConcept { get; }

        internal AttributeDef(ConceptDef parentConcept, string name, Type cstype)
        {
            CSType = cstype;
            Name = name;
            ParentConcept = parentConcept;
        }

        override internal AttributeInstanceBase CreateInstance(ConceptInstance conceptInstance)
        {
            return new AttributeInstance(this, conceptInstance);
        }
    }

    public class ComputableAttributeDef : AttributeDefBase
    {
        public ICompiledExpression Expr { get; private set; }

        override public Type CSType { get; }

        override public string Name { get; }

        override public ConceptDef ParentConcept { get; }

        public IReadOnlyList<AttributeDefBase> Dependencies { 
            get 
            {
                var funcArgs = new List<AttributeDefBase>();
                foreach (var arg in Expr.Args)
                {
                    if (ParentConcept.ConceptManager.TryGetConceptDef(arg.Path[0], out var concept))
                    {
                        funcArgs.Add(concept.GetAttribute(arg.Path[1]));
                    }
                    else
                    {
                        funcArgs.Add(ParentConcept.GetAttribute(arg.Path[0]));
                    }
                }
                return funcArgs;
            }
        }

        internal ComputableAttributeDef(ConceptDef parent, string name, Type type, ICompiledExpression expr)
        {
            ParentConcept = parent;
            Name = name;
            CSType = type;
            Expr = expr;

            
            foreach (var dep in Dependencies)
            {
                dep.AddDependentAttr(new DependentAttrInfo(ParentConcept.Name, this.Name));
            }
        }

        override internal AttributeInstanceBase CreateInstance(ConceptInstance conceptInstance)
        {
            return new ComputableAttributeInstance(this, conceptInstance);
        }
    }

    public abstract class AttributeInstanceBase
    {
        public abstract AttributeDefBase Definition { get; }
        // public abstract object? Value { get; }
        public abstract ConceptInstance ParentConcept { get; }
        public abstract object? Value { get; internal set;}
        public abstract void SetCahcedValue(object? value);
        public List<ComputableAttributeInstance> GetDependentAttrs()
        {
            return Definition.DependentAttrs
                .Select(x => (ComputableAttributeInstance?)(ParentConcept.Group.GetInstanceOrNull(x.ParentConcept)?.GetAttribute(x.Name)))
                .Where(x => x != null)
                .ToList();
        }

        public void NotifyAboutValueChange()
        {
            GetDependentAttrs().ForEach(x => x.RecalcValue());
        }
    }

    public class AttributeInstance : AttributeInstanceBase
    {
        private AttributeDef _defenition;
        public override AttributeDef Definition => _defenition;
        public override ConceptInstance ParentConcept { get; }
        private object? _value;
        public override object? Value
        {
            get => _value;
            internal set
            {
                _value = TypeConversionUtils.ChangeTypeToNullable(value, _defenition.CSType);
                NotifyAboutValueChange();
            }
        }

        internal AttributeInstance(AttributeDef definition, ConceptInstance parentConcept)
        {
            _defenition = definition;
            ParentConcept = parentConcept;
            SetCahcedValue(null);
        }

        public override void SetCahcedValue(object? value)
        {
            _value = TypeConversionUtils.ChangeTypeToNullable(value, _defenition.CSType);
        }
    }

    public class ComputableAttributeInstance : AttributeInstanceBase
    {
        public ComputableAttributeDef _definition;
        public override ComputableAttributeDef Definition => _definition;
        public override ConceptInstance ParentConcept { get; }
        private object? _cachedValue = null;
        private bool _isValueInitialized = false;

        internal ComputableAttributeInstance(ComputableAttributeDef definition, ConceptInstance parentConcept)
        {
            _definition = definition;
            ParentConcept = parentConcept;
        }

        public override object? Value 
        {
            get => _isValueInitialized ? _cachedValue : RecalcValue();
            internal set => throw new NotSupportedException("Cannot set value to computable attribute");
        }

        public object? RecalcValue()
        {
            var value = _definition.Expr.Func(
            _definition.Dependencies.Select(
                dep => ParentConcept.Group.GetInstance(dep.ParentConcept).GetAttribute(dep.Name).Value
                ).ToArray()
            );
            SetCahcedValue(TypeConversionUtils.ChangeTypeToNullable(value, Definition.CSType));
            NotifyAboutValueChange();
            return _cachedValue;
        }

        public List<AttributeInstanceBase> GetDependencies()
        {
            return _definition.Dependencies.Select(x => ParentConcept.Group.GetInstance(x.ParentConcept).GetAttribute(x.Name)).ToList();
        }

        public override void SetCahcedValue(object? value)
        {
            _isValueInitialized = true;
            _cachedValue = TypeConversionUtils.ChangeTypeToNullable(value, Definition.CSType);
        }

        // object? AttributeInstanceBase.Value { get => Value; set => throw new NotSupportedException("Cannot set value to computable attribute"); }
    }

    // public class CachedAttrInstance : AttributeInstanceBase
    // {
    //     public AttributeDef Definition;
    //     AttributeDefBase AttributeInstanceBase.Definition => Definition;
    //     public object Value { get; }
    //     object? AttributeInstanceBase.Value { get => Value; set => throw new NotImplementedException(); }

    //     internal CachedAttrInstance(AttributeDef definition, object value)
    //     {
    //         Definition = definition;
    //         Value = value;
    //     }
    // }

    // public class CachedConceptInstace : IConceptInstance
    // {
    //     public ConceptDef Definition { get; private set; }

    //     public bool AreDependenciesLoaded { get; private set; } = false;
    //     private Dictionary<string, CachedAttrInstance> _attributes;


    //     public CachedConceptInstace(ConceptDef definition, IEnumerable<CachedAttrInstance> cachedAttrInstances)
    //     {
    //         Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    //         _attributes = cachedAttrInstances.ToDictionary(x => x.Definition.Name);
    //         Utils.CompareDictKeys(_attributes, Definition.Attributes);
    //     }

    //     public object? GetAttributeValue(string name)
    //     {
    //         return GetAttribute(name).Value;
    //     }

    //     public AttributeInstanceBase GetAttribute(string name)
    //     {
    //         if (!Definition.Attributes.ContainsKey(name))
    //         {
    //             throw new ArgumentException($"No such attribute with name {name}");
    //         }
    //         if (!_attributes.ContainsKey(name))
    //         {
    //             throw new InvalidOperationException($"Attribute has been added to definition after concept initialization {name}");
    //         }
    //         return _attributes[name];
    //     }

    //     public ConceptInstance GetInstanceWithDeps()
    //     {
    //         throw new NotImplementedException();
    //     }
    // }

    public static class TypeConversionUtils
    {
        public static object? ChangeTypeToNullable(object? value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                return Convert.ChangeType(value, underlyingType);
            }
            return Convert.ChangeType(value, targetType);
        }
    }
}

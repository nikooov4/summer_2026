using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.Core.UI.Attributes;

//using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints;

namespace Mephi.Cybernetics.Race.Core
{
    public class PropertyDescription :  IDomainAttribute//, IAttributeDescription<ObjectDescription,ObjectDescription>
    {
        public delegate TResult Func<in TAgr, out TResult>(TAgr arg);
        public delegate TResult Func<in TAgr1, in TArg2, out TResult>(TAgr1 arg1, TArg2 arg2);
        public delegate void Action<in TAgr>(TAgr arg);
        public delegate void Action<in TAgr1, in TArg2>(TAgr1 arg1, TArg2 arg2);

        private static int counter = 0;

        public static Func<object, object> GetComputablePropertyGetter(PropertyInfo propertyInfo)
        {
            if (DynamicMethodSupports(propertyInfo))
                return propertyInfo.GetMethod.IsStatic ? CreateStaticGetterFromDynamicMethod(propertyInfo) : CreateGetterFromDynamicMethod(propertyInfo);
            else
            {
                if (propertyInfo.GetMethod.IsStatic)
                {
                    var staticGenericGetterCreator = typeof(PropertyDescription)
                        .GetMethod("StaticPropertyGetterGenericWrapper", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(propertyInfo.ReflectedType, propertyInfo.PropertyType);
                    return
                        (Func<object, object>)staticGenericGetterCreator.Invoke(null, new object[] { propertyInfo.GetGetMethod() });
                }
                else
                {
                    var genericGetterCreator = typeof(PropertyDescription)
                        .GetMethod("PropertyGetterGenericWrapper", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(propertyInfo.ReflectedType, propertyInfo.PropertyType);
                    return
                        (Func<object, object>)genericGetterCreator.Invoke(null, new object[] { propertyInfo.GetGetMethod() });
                }
            }
        }

        private static Func<object, object> PropertyGetterGenericWrapper<TInstance, TType>(MethodInfo method)
        {
            var genericGetter = (Func<TInstance, TType>)Delegate.CreateDelegate(typeof (Func<TInstance, TType>), method);
            Func<object, object> getter = o => genericGetter((TInstance)o);
            return getter;
        }

        private static Func<object, object> StaticPropertyGetterGenericWrapper<TInstance, TType>(MethodInfo method)
        {
            var genericGetter = (Func<TType>)Delegate.CreateDelegate(typeof(Func<TType>), method);
            Func<object, object> getter = o => genericGetter();
            return getter;
        }

        private static bool DynamicMethodSupports(PropertyInfo propertyInfo)
        {
            return !(propertyInfo.ReflectedType.ContainsGenericParameters || propertyInfo.ReflectedType.IsArray ||
                     propertyInfo.ReflectedType.IsInterface || propertyInfo.ReflectedType.IsGenericParameter);
        }

        private static Func<object, object> CreateGetterFromDynamicMethod(PropertyInfo propertyInfo)
        {
            counter++;
            // The second version copied from http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/ with slight corrections
            // concerning properties of reference types
            var arguments = new Type[] {typeof (object)};

            var getter = new DynamicMethod(String.Concat("_Get", propertyInfo.Name, "_"), typeof (object), arguments,
                                           propertyInfo.ReflectedType);
            var generator = getter.GetILGenerator();
            generator.DeclareLocal(typeof (object));

            // Loading the first (and only) argument to the execution stack and downcasting it
            // Note that a specific downcasting method is required for a reference-type object property
            generator.Emit(OpCodes.Ldarg_0);
            if ( propertyInfo.ReflectedType.IsClass || propertyInfo.ReflectedType.IsInterface )
                generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            else
                generator.Emit(OpCodes.Unbox, propertyInfo.ReflectedType);

            // Call the property getter
            generator.EmitCall(OpCodes.Call, propertyInfo.GetGetMethod(true), null);

            if ( !(propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsInterface) )
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

            //var memoryDelta = Process.GetCurrentProcess().PrivateMemorySize64;
            generator.Emit(OpCodes.Ret);
            //memoryDelta = Process.GetCurrentProcess().PrivateMemorySize64 - memoryDelta;

            return (Func<object, object>)getter.CreateDelegate(typeof (Func<object, object>));
        }

        private static Action<object, object> PropertySetterGenericWrapper<TInstance, TValue>(MethodInfo method)
        {
            var genericSetter =
                (Action<TInstance, TValue>)Delegate.CreateDelegate(typeof (Action<TInstance, TValue>), method);
            Action<object, object> setter = (o, v) => genericSetter((TInstance)o, (TValue)v);
            return setter;
        }

        private static Action<object, object> GetComputablePropertySetter(PropertyInfo propertyInfo)
        {
            //var genericGetterCreator = typeof(PropertyDescription).GetMethod("GetComputablePropertySetterGeneric", BindingFlags.Static | BindingFlags.NonPublic)
            //                                                        .MakeGenericMethod(propertyInfo.ReflectedEntityDescription, propertyInfo.PropertyType);
            //return (Action<object, object>)genericGetterCreator.Invoke(null, new object[] { propertyInfo });

            if ( DynamicMethodSupports(propertyInfo) )
                return propertyInfo.GetMethod.IsStatic ? GetStaticSetterFromDynamicMethod(propertyInfo) : GetSetterFromDynamicMethod(propertyInfo);
            else
            {
                var genericSetterCreator = typeof (PropertyDescription)
                    .GetMethod(nameof(PropertySetterGenericWrapper), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(propertyInfo.ReflectedType, propertyInfo.PropertyType);
                return
                    (Action<object, object>)
                    genericSetterCreator.Invoke(null, new object[] {propertyInfo.GetSetMethod()});
            }
        }

        private static Action<object, object> GetSetterFromDynamicMethod(PropertyInfo propertyInfo)
        {
            // The second version copied from http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/ with slight corrections
            // concerning properties of reference types
            var setMethod = propertyInfo.GetSetMethod();
            if ( setMethod == null )
                return null;

            var arguments = new Type[] {typeof (object), typeof (object)};

            var setter = new DynamicMethod(String.Concat("_Set", propertyInfo.Name, "_"), typeof (void), arguments,
                                           propertyInfo.DeclaringType);
            var generator = setter.GetILGenerator();

            // Loading the first argument to the execution stack and downcasting it
            // Note that a specific downcasting method is required for a reference-type object property
            generator.Emit(OpCodes.Ldarg_0);
            if ( propertyInfo.ReflectedType.IsClass || propertyInfo.ReflectedType.IsInterface )
                generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            else
                generator.Emit(OpCodes.Unbox, propertyInfo.ReflectedType);

            // Loading the second argument on the current execution stack and perform the required unboxing
            generator.Emit(OpCodes.Ldarg_1);
            if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsInterface)
                generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            else
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

            // Call the property setter
            generator.EmitCall(OpCodes.Call, setMethod, null);

            // return
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)setter.CreateDelegate(typeof (Action<object, object>));
        }

        private static Func<object,object> CreateStaticGetterFromDynamicMethod(PropertyInfo propertyInfo)
        {
            var arguments = new Type[] { typeof(object) };

            var getter = new DynamicMethod(String.Concat("_Get", propertyInfo.Name, "_"), typeof(object), arguments, propertyInfo.ReflectedType);
            var generator = getter.GetILGenerator();

            // Call the property getter
            generator.EmitCall(OpCodes.Call, propertyInfo.GetGetMethod(true), null);

            if (!(propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsInterface))
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

            //var memoryDelta = Process.GetCurrentProcess().PrivateMemorySize64;
            generator.Emit(OpCodes.Ret);
            //memoryDelta = Process.GetCurrentProcess().PrivateMemorySize64 - memoryDelta;

            return (Func<object,object>)getter.CreateDelegate(typeof(Func<object,object>));
        }

        private static Action<object, object> GetStaticSetterFromDynamicMethod(PropertyInfo propertyInfo)
        {
            // The second version copied from http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/ with slight corrections
            // concerning properties of reference types
            var setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
                return null;

            var arguments = new Type[] {typeof(object), typeof(object)};

            var setter = new DynamicMethod(String.Concat("_Set", propertyInfo.Name, "_"), typeof(void), arguments,
                propertyInfo.DeclaringType);
            var generator = setter.GetILGenerator();

            // Loading the second argument on the current execution stack and perform the required unboxing
            generator.Emit(OpCodes.Ldarg_1);
            if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsInterface)
                generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            else
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

            // Call the property setter
            generator.EmitCall(OpCodes.Call, setMethod, null);

            // return
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>) setter.CreateDelegate(typeof(Action<object, object>));
        }

        private class DefaultValue<T>
        {
            public T Value = default(T);
        }

        private readonly ObjectDescription _domain;
        private ObjectDescription _range;
        private readonly PropertyInfo _property;
        private readonly int _index;
        private readonly AttributeContainer _attributeContainer;
        private readonly bool _isStatic;
        private readonly bool _isReadOnly;
        private object _propertyDefaultValue;
        private readonly bool _isComputable;
        private readonly ComputableAttribute _computableAttribute;
        private readonly Func<object, object> _propertyGetter;
        private readonly Action<object, object> _propertySetter;

        private readonly Func<object, object> _getValueWithoutChecks;
        private readonly Func<object, object> _getValue;
        private readonly bool _isOverloadable;
        private bool _defaultValueCached;
        private bool _isList;
        private bool _isIList;


        internal PropertyDescription(ObjectDescription entityDescription, PropertyInfo property, int index)
        {
            _isStatic = property.GetMethod.IsStatic;
            _domain = entityDescription;
            _property = property;
            SetPropertyType();
            _index = index;
            _attributeContainer = new AttributeContainer(_property.GetCustomAttributes(true));

            if ( !(_property.IsSpecialName || _property.Name == "Item") && _property.CanRead )
                _propertyGetter = GetComputablePropertyGetter(_property);
            if ( !(_property.IsSpecialName || _property.Name == "Item") && _property.CanWrite )
                _propertySetter = GetComputablePropertySetter(_property);

            _isOverloadable = _property.GetGetMethod(true).IsVirtual && !_property.GetGetMethod(true).IsFinal;
                // id getter is virtual, so would be the setter

            _isReadOnly = _propertySetter == null;

            foreach ( var attr in _attributeContainer )
            {
                if ( attr is ComputableAttribute computableAttribute)
                {
                    if (!IsReadOnly)
                        throw new InvalidOperationException("Dunno wat toodoo");
                    _isComputable = true;
                    _computableAttribute = computableAttribute;
                }
            }

            GetDefaultPropertyValue();

            // Установим _getValueWithoutCheck -- работает, когда тип объекта тот же, что и тип данного свойства
            if ( !IsComputable )
                    _getValueWithoutChecks = _propertyGetter;
            else //TODO: подумать насчет кеширования значений для вычисляемых свойств; вообще подумать на предмет транзакционной памяти
                _getValueWithoutChecks = PropertyGetter;

            if ( !IsOverloadable )
            {
                _getValue = _getValueWithoutChecks;
            }
            else if ( IsOverloadable )
            {
                if ( this.Domain.ClrType.IsInterface )
                {
                    _getValue = o => ObjectDescription.Get(o.GetType()).InterfaceMapping[this]._getValueWithoutChecks(o);
                }
                else
                    _getValue = o =>
                                    {
                                        var type = o.GetType();
                                        return type != this.Domain.ClrType
                                                   ? (ObjectDescription.Get(type).Properties[Index]._getValueWithoutChecks(o))
                                                   // add 'type <= ReflectedEntity.ClrType' check
                                                   : _getValueWithoutChecks(o);
                                    };
            }


        }

        public int Index
        {
            get { return _index; }
        }

        public PropertyInfo Property
        {
            get { return _property; }
        }

        public string Name
        {
            get { return _property.Name; }
        }

        public ObjectDescription Range
        {
            get { return _range ?? (_range = ObjectDescription.Get(_property.PropertyType)); }
        }

        IDomain IAttributeBase2<IDomain, IDomain>.Range => Range;

        public Type PropertyType
        {
            get { return _property.PropertyType; }
        }

        //TODO: вернуть internal
        public AttributeContainer Attributes
        {
            get { return _attributeContainer; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public Type RangeType
        {
            get { return Range.ClrType; }
        }

        public ObjectDescription RelatedTypeDescription
        {
            get { return ObjectDescription.Get(RangeType); }
        }

        public ObjectDescription Domain
        {
            get { return _domain; }
        }

        IDomain IAttributeBase2<IDomain, IDomain>.Domain => Domain;

        public object PropertyDefaultValue
        {
            get { return GetDefaultPropertyValue(); }
        }

        public bool IsComputable
        {
            get { return _isComputable; }
        }

        public ComputableAttribute ComputableInfo
        {
            get { return _computableAttribute; }
        }

        public bool IsOverloadable
        {
            get { return _isOverloadable; }
        }

        public Func<object, object> PropertyGetter
        {
            get { return _propertyGetter; }
        }


        public Attribute GetAttribute(Type t)
        {
            return _attributeContainer.GetAttribute(t);
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return _attributeContainer.GetAttribute<T>();
        }

        public bool HasAttribute<T>() where T : Attribute
        {
            return this.GetAttribute<T>() != null;
        }

        public bool HasAttribute(Type attributeType)
        {
            return this.GetAttribute(attributeType) != null;
        }

        public object GetValue(object o)
        {
            return _getValue(o);
        }

        public T GetValue<T>(object o)
        {
            return (T)_getValue(o);
        }

        public void SetValue<T>(object o, T value)
        {
            SetValue(o, value as object);
        }

        //        //TODO: Исправить, реализовать такой механизм, как для "обычного" GetValue
//        public object GetValue(object o, params object[] index)
//        {
//            return _property.GetValue(o, index);
//        }
//
//        //TODO: Исправить, реализовать такой механизм, как для "обычного" GetValue
//        public T GetValue<T>(object o, params object[] index)
//        {
//            return (T)_property.GetValue(o, index);
//        }

        public void SetValue(object o, object value)
        {
            if ( value == DBNull.Value /*|| value is UnasignedValue*/ )
                value = GetDefaultPropertyValue();
            _propertySetter(o, value);
        }

//        public List<IEntity> GetAllRelatedValues(PersistentStorage ps)
//        {
//            return new List<IEntity>(ps.GetEntities(this.RangeType));
//        }

        public object GetDefaultPropertyValue()
        {
            if ( _defaultValueCached )
                return _propertyDefaultValue;
            else
            {
                _defaultValueCached = true;
                if ( PropertyType.IsArray )
                {
                    _propertyDefaultValue = CollectionDescription.CreateArrayEmpty(PropertyType);
                }
                else if ( PropertyType.IsGenericType && _isList )
                {
                    _propertyDefaultValue = PropertyType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                }
//                else if (PropertyType.IsGenericType && _isIList && typeof(IEntity).IsAssignableFrom(PropertyType.GetGenericArguments()[0]))
//                {
//                    _propertyDefaultValue = typeof(EntityList<>).MakeGenericType(PropertyType.GetGenericArguments()[0])
//                                                .GetConstructor(new Type[] {})
//                                                .Invoke(new object[] {});
//                }
                else
                {
                    if ( _isIList )
                        _propertyDefaultValue = new ArrayList();
                    else if ( typeof(IList).IsAssignableFrom(PropertyType) )
                        _propertyDefaultValue = new ArrayList();
                    else
                    {
                        var tmp = typeof(DefaultValue<>).MakeGenericType(PropertyType).GetConstructor(new Type[0]).Invoke(null);
                        _propertyDefaultValue = tmp.GetType().GetField("Value").GetValue(tmp);
                    }
                }
                return _propertyDefaultValue;
            }
        }

        public T GetDefaultPropertyValue<T>()
        {
            return (T)GetDefaultPropertyValue();
        }

        private void SetPropertyType()
        {
            _isIList = typeof (IList).IsAssignableFrom(PropertyType);
            _isList = _isIList && PropertyType.IsGenericType &&
                      PropertyType == typeof (List<>).MakeGenericType(PropertyType.GetGenericArguments()[0]);
        }


        public override string ToString()
        {
            return String.Format("property {2} {1}.{0}", Name, Domain.ClrType.Name, PropertyType.Name);
        }
    }
}

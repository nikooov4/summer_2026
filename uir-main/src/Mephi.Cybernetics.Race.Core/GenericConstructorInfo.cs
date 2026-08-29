using System;
using System.Collections.Generic;
using System.Linq;
using Mephi.Cybernetics.Race.Core.TypeSystem;

//using Mephi.Cybernetics.Race.HostEnvironments.TypeSystem;

namespace Mephi.Cybernetics.Race.Core;

public class GenericDomainConstructorInfo<TDomain> : IConstructor<TDomain> where TDomain : IDomain
{
    private Dictionary<string, SignatureArgumentInfo<TDomain>> _genericArgumentsByName;
    private SignatureArgumentInfo<TDomain>[] _genericArgumentsByIndex;
    private readonly TDomain _genericType;

    internal GenericDomainConstructorInfo(string source, string @namespace, string name, IEnumerable<KeyValuePair<string,TDomain>> typeArguments)
    {
        Source = source;
        Namespace = @namespace;
        Name = name;
        _genericArgumentsByName = new Dictionary<string, SignatureArgumentInfo<TDomain>>();
        int counter = 0;
        foreach (var typeArgument in typeArguments)
        {
            var argInfo = new SignatureArgumentInfo<TDomain>(counter, typeArgument.Key, typeArgument.Value, false, false);
            _genericArgumentsByName.Add(typeArgument.Key, argInfo);
            _genericArgumentsByIndex[counter++] = argInfo;
        }
    }

#if NET5_0_OR_GREATER
    internal GenericDomainConstructorInfo(string source, string @namespace, string name, IEnumerable<(string,TDomain)> typeArguments)
    {
        Source = source;
        Namespace = @namespace;
        Name = name;
        _genericArgumentsByName = new Dictionary<string, SignatureArgumentInfo<TDomain>>();
        int counter = 0;
        foreach ((var argName, var typeArgument) in typeArguments)
        {
            var argInfo = new SignatureArgumentInfo<TDomain>(counter, argName, typeArgument, false, false);
            _genericArgumentsByName.Add(argName, argInfo);
            _genericArgumentsByIndex[counter++] = argInfo;
        }

    }
#endif

    public SignatureArgumentInfo<TDomain> this[int index] => _genericArgumentsByIndex[index];
    IArgumentInfo<TDomain> ISignature<TDomain, IArgumentInfo<TDomain>>.this[int index] => _genericArgumentsByIndex[index];

    public SignatureArgumentInfo<TDomain> this[string name] => _genericArgumentsByName[name];
    IArgumentInfo<TDomain> ISignature<TDomain, IArgumentInfo<TDomain>>.this[string name] => _genericArgumentsByName[name];

    public string Name { get; }

    public TDomain ReturnType => throw new NotImplementedException();

    public IArgumentInfo<TDomain>[] Arguments => throw new NotImplementedException();

    public int Arity => _genericArgumentsByIndex.Length;

    public string Source { get; }

    public string Namespace { get; }

    public TDomain Construct(IList<TDomain> args)
    {
        throw new NotImplementedException();
    }

    public TDomain Construct(TDomain[] args)
    {
        throw new NotImplementedException();
    }
}

//TODO: как-то пропихнуть genericType в SignatureArgument
public class GenericClrTypeConstructorInfo : IConstructor<Type>
{
    private Dictionary<string, SignatureArgumentInfo<Type>> _genericArgumentsByName;
    private SignatureArgumentInfo<Type>[] _genericArgumentsByIndex;
    private readonly Type _genericType;

    public GenericClrTypeConstructorInfo(Type genericType) 
    {
        if (!genericType.IsGenericTypeDefinition)
            throw new ArgumentException("The System.Type must a type constructor");
        _genericType = genericType;
        int index = 0;
        foreach (var genericArgument in _genericType.GetGenericArguments())
        {
            var arg = new SignatureArgumentInfo<Type>(index, genericArgument.Name, typeof(Type), false, false);
            _genericArgumentsByIndex[index] = arg;
            _genericArgumentsByName[arg.Name] = arg;

        }    
    }

    public SignatureArgumentInfo<Type> this[int index] => _genericArgumentsByIndex[index];
    IArgumentInfo<Type> ISignature<Type, IArgumentInfo<Type>>.this[int index] => _genericArgumentsByIndex[index];

    public SignatureArgumentInfo<Type> this[string name] => _genericArgumentsByName[name];
    IArgumentInfo<Type> ISignature<Type, IArgumentInfo<Type>>.this[string name] => _genericArgumentsByName[name];

    public string Name => $"{_genericType.Namespace}.{_genericType.Name}<>";

    public Type ReturnType => typeof(Type);

    public IArgumentInfo<Type>[] Arguments => throw new NotImplementedException();

    public int Arity => _genericArgumentsByIndex.Length;

    public Type Construct(IList<Type> args)
    {
        throw new NotImplementedException();
    }

    public Type Construct(Type[] args)
    {
        throw new NotImplementedException();
    }
}
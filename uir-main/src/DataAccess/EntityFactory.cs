using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using System.Threading;
using Mephi.Cybernetics.Race.Core;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public class EntityFactory ///TODO: rename DomainInfo
	{
        //private readonly PersistentStorage _ps;
		private Dictionary<string, EntityDescription> _metaObjects = new Dictionary<string, EntityDescription>();
		private Dictionary<string, ObjectFactoryDescription> _objectFactories = new Dictionary<string, ObjectFactoryDescription>();

	    public EntityFactory(PersistentStorage ps)
	    {
            //_ps = ps;
	    }

        //public PersistentStorage Ps
        //{
        //    get { return _ps; }
        //}

        //public FacadeBuilder GetFacadeBuilder(Type entityType)
        //{
        //    return Ps.GetFacadeBuilder(entityType);
        //}

		public EntityDescription GetObjectDescription(Type type)
		{
			if ( !_metaObjects.ContainsKey(type.ToString()) )
                _metaObjects.Add(type.ToString(), EntityDescription.GetEntityDescription(type));
            return _metaObjects[type.ToString()];
		}

		public InstanceDescription GetInstanceDescription(IEntity entity)
		{
			return new InstanceDescription(entity);
		}

		public void SetObjectFactory(Type type, IObjectFactory factory)
		{
            if (_objectFactories.ContainsKey(type.ToString()))
                _objectFactories[type.ToString()] = new ObjectFactoryDescription(factory);
			else
                _objectFactories.Add(type.ToString(), new ObjectFactoryDescription(factory));
		}

		public void SetObjectFactory(Type type, DefaultConstruction defaultConstruction,
			ConstructionWithArgs constructionWithArgs)
		{
			ObjectFactoryDescription fd = new ObjectFactoryDescription(defaultConstruction, constructionWithArgs);
            if (_objectFactories.ContainsKey(type.ToString()))
                _objectFactories[type.ToString()] = fd;
			else
                _objectFactories.Add(type.ToString(), fd);
		}

		public ObjectFactoryDescription GetObjectFactory(Type type)
		{
            if (!_objectFactories.ContainsKey(type.ToString()))
                _objectFactories.Add(type.ToString(), new ObjectFactoryDescription(new DefaultObjectFactory()));
            return _objectFactories[type.ToString()];
		}

        //public static void SetThreadDefaultPs(Thread thread, PersistentStorage _ps)
        //{
        //    _currentThreadPs[thread] = _ps;
        //}

        //public bool PsExists(string key)
        //{
        //    return _ps.ContainsKey(key);
        //}

        //public static PersistentStorage GetThreadDefaultPs(Thread thread)
        //{
        //    return _currentThreadPs[thread];
        //}

        //public static PersistentStorage CreatePersistentStorage(DataSet dataSet)
        //{
        //    lock ( _ps )
        //    {
        //        if ( !_ps.ContainsKey(dataSet) )
        //            _ps.AddProperty(dataSet, new PersistentStorage(dataSet, null));
        //        if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
        //            _currentThreadPs.AddProperty(System.Threading.Thread.CurrentThread, _ps[dataSet]);
        //        else
        //            _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[dataSet];
        //        return _ps[dataSet];
        //    }
        //}

        //public static PersistentStorage CreatePersistentStorage(DataSet dataSet, string configuration)
        //{
        //    lock ( _ps )
        //    {
        //        if ( !_ps.ContainsKey(dataSet) )
        //            _ps.AddProperty(dataSet, new PersistentStorage(dataSet, configuration));
        //        if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
        //            _currentThreadPs.AddProperty(System.Threading.Thread.CurrentThread, _ps[dataSet]);
        //        else
        //            _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[dataSet];
        //        return _ps[dataSet];
        //    }
        //}

        //public static PersistentStorage CreatePersistentStorage(string key, DataSet dataSet, string configuration)
        //{
        //    lock ( _ps )
        //    {
        //        if ( !_ps.ContainsKey(key) )
        //            _ps.AddProperty(key, new PersistentStorage(dataSet, configuration));
        //        if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
        //            _currentThreadPs.AddProperty(System.Threading.Thread.CurrentThread, _ps[key]);
        //        else
        //            _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[key];
        //        return _ps[key];
        //    }
        //}

        //public static PersistentStorage CreatePersistentStorage(string key, string connectionString)
        //{
        //    lock ( _ps )
        //    {
        //        if ( !_ps.ContainsKey(key) )
        //            _ps.AddProperty(key, new PersistentStorage(connectionString, null));
        //        if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
        //            _currentThreadPs.AddProperty(System.Threading.Thread.CurrentThread, _ps[key]);
        //        else
        //            _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[key];
        //        return _ps[key];
        //    }
        //}

        //public static PersistentStorage CreatePersistentStorage(string key, string connectionString, string configuration)
        //{
        //    lock ( _ps )
        //    {
        //        if ( !_ps.ContainsKey(key) )
        //            _ps.AddProperty(key, new PersistentStorage(connectionString, configuration));
        //        if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
        //            _currentThreadPs.AddProperty(System.Threading.Thread.CurrentThread, _ps[key]);
        //        else
        //            _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[key];
        //        return _ps[key];
        //    }
        //}

        //public static PersistentStorage CreatePersistentStorage(string key)
        //{
        //    //if (!_ps.ContainsKey(key))
        //    //    _ps.AddProperty(key, new PersistentStorage(connectionString));
        //    //if (_currentThreadPs[System.Threading.Thread.CurrentThread] == null)
        //    //    _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[key];
        //    return _ps[key];
        //}

        //public static PersistentStorage CreatePersistentStorage(string key, string fileName, string entitiesTagPath, string configuration)
        //{
        //    if (!_ps.ContainsKey(key))
        //        _ps.AddProperty(key, new PersistentStorage(fileName, entitiesTagPath, configuration));
        //    if ( _currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
        //        _currentThreadPs.AddProperty(System.Threading.Thread.CurrentThread, _ps[key]);
        //    return _ps[key];
        //}

        //public static PersistentStorage CreatePersistentStorage(IConnection connection)
        //{
        //    if ( !_ps.ContainsKey(connection) )
        //        _ps.AddProperty(connection, new PersistentStorage(new Adapter(connection)));
        //    return _ps[connection];
        //}

        //public static void DeletePersistentStorage(DataSet key)
        //{
        //    PersistentStorage _ps = _ps[key];
        //    _ps.Dispose();
        //    _ps.Remove(key);
        //}

        //public static void DeletePersistentStorage(string key)
        //{
        //    PersistentStorage _ps = _ps[key];
        //    _ps.Dispose();
        //    _ps.Remove(key);
        //}

        //public object CreateObject(Type type)
        //{
        //    if (!_metaObjects.ContainsKey(type.ToString()))
        //        _metaObjects.AddProperty(type.ToString(), new EntityDescription(type));
        //    return _metaObjects[type.ToString()].CreateObject();
        //}

        //public object CreateObject(Type type, params object[] args )
        //{
        //    if (!_metaObjects.ContainsKey(type.ToString()))
        //        _metaObjects.AddProperty(type.ToString(), new EntityDescription(type));
        //    return _metaObjects[type.ToString()].CreateObject(args);
        //}

        //public T CreateObject<T>()
        //{
        //    if (!_metaObjects.ContainsKey(typeof(T).ToString()))
        //        _metaObjects.AddProperty(typeof(T).ToString(), new EntityDescription(typeof(T)));
        //    T o = (T)_metaObjects[typeof(T).ToString()].CreateObject();
        //    (o as EntityBase).OwnerPs = Ps;
        //    return o;
        //}

        //public T CreateObject<T>(params object[] args)
        //{
        //    if ( !_metaObjects.ContainsKey(typeof(T).ToString()) )
        //        _metaObjects.AddProperty(typeof(T).ToString(), new EntityDescription(typeof(T)));
        //    T o = (T)_metaObjects[typeof(T).ToString()].CreateObject(args);
        //    (o as EntityBase).OwnerPs = Ps;
        //    return o;
        //}
	}
}

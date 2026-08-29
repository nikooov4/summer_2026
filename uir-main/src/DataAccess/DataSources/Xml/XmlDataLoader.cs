using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using System.Collections;
using Mephi.Cybernetics.Race.DataAccess.DataSources.MetaData;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Xml
{
	public class XmlDataLoader : ILoader
	{
		private readonly EntityDescription _entityDescription;
		private readonly Mapper _mapper;
		private readonly XmlReader _sourceReader;
        private XmlReaderSettings _sett;
        private string _fileName;
        private string _entitiesTagPath;
	    private readonly XmlDataSource _dataSource;
	    private readonly PersistentStorage _ps;

        private XmlDataLoader(Mapper mapper, XmlDataSource dataSource, PersistentStorage ps)
        {
            _mapper = mapper;// dataSource.GetMapper(type);
            _entityDescription = mapper.MappedEntity;
            _ps = ps;
            _dataSource = dataSource;
        }

        public XmlDataLoader(XmlReader reader, Mapper mapper, XmlDataSource dataSource, PersistentStorage ps) : 
            this(mapper, dataSource, ps)
		{
			_sourceReader = reader;
        }

        public XmlDataLoader(string fileName, string entitiesTagPath, Mapper mapper, XmlDataSource dataSource, PersistentStorage ps)
            : this(mapper, dataSource, ps)

		{
            _fileName = fileName;
            _entitiesTagPath = entitiesTagPath;
			//StreamReader streamReader = new StreamReader(fileName);
            _sett = new XmlReaderSettings
                        {
                            CloseInput = false,
                            IgnoreComments = true,
                            IgnoreProcessingInstructions = true,
                            IgnoreWhitespace = true
                        };
            _sourceReader = XmlReader.Create(fileName, _sett);
            XmlUtils.NavigateThisToNode(_sourceReader, entitiesTagPath);
            XmlUtils.NavigateThisToNode(_sourceReader, Mapper.TableName);
        }

		public EntityDescription ReflectedType
		{
			get
			{
				return _entityDescription;
			}
		}

		public Mapper Mapper
		{
			get
			{
				return _mapper;
			}
		}


        public List<T> LoadAll<T>() where T : IEntity
		{
			return new List<T>();
		}

        public List<IEntity> LoadAllOld()
		{
            var list = new List<IEntity>();
            if (_sourceReader.NodeType != XmlNodeType.Element)
                return list;
            using (var reader = _sourceReader.ReadSubtree())
            {
                reader.Read();

                if (string.IsNullOrEmpty(reader.Name) && reader.NodeType == XmlNodeType.None)
                    return list;

                reader.ReadToDescendant(ReflectedType.Object.ClrType.Name);
                // Now we are at the first record of the needed DataTable

                while (!reader.EOF)
                {
                    if (reader.IsEmptyElement || reader.NodeType == XmlNodeType.EndElement)
                        break;
                    //XmlReader rdr = reader.ReadSubtree();// get the subtree of the entity instance
                    //rdr.Read();// Get the root node of the entity instance
                    IEntity entity = ReadObject(reader, ReflectedType);
                    //SkipWhiteSpace(reader);
                    _ps.Caches[ReflectedType].Add(entity);
                    reader.ReadToFollowing(ReflectedType.Object.ClrType.Name);
                    list.Add(entity);
                }
                SetOtoRelations(list);
                SetOtmRelations(list);
                SetMtmRelations(list);
                return list;
            }
		}

        public List<IEntity> LoadAll()
        {
            var list = new List<IEntity>();
            if (_sourceReader.NodeType != XmlNodeType.Element)
                return list;
            using (var reader = _sourceReader.ReadSubtree())
            {
                reader.Read();
                reader.Read();
                SkipWhiteSpace(reader);
                // Добавить GetObjectDescription на основе метаданных из xml
                var od = new DataObjectDescription(ReflectedType.Object.ClrType, _dataSource.GetTable(ReflectedType));

                if (string.IsNullOrEmpty(reader.Name) && reader.NodeType == XmlNodeType.None)
                    return list;

                //reader.ReadToDescendant(od.Name);
                // Now we are at the first record of the needed DataTable

                while (!reader.EOF)
                {
                   // if (reader.IsEmptyElement || reader.NodeType == XmlNodeType.EndElement)
                   //     break;
                    if (reader.GetAttribute("name") != od.DataTable.Name)
                    {
                        reader.Read();
                        continue;
                    }
                    //XmlReader rdr = reader.ReadSubtree();// get the subtree of the entity instance
                    //rdr.Read();// Get the root node of the entity instance
                    
                    IEntity entity = ReadObject(reader, od);
                    //SkipWhiteSpace(reader);
                    _ps.Caches[ReflectedType].Add(entity);
                    reader.ReadToFollowing(od.Name);
                    list.Add(entity);
                }
               // SetOtoRelations(list);
               // SetOtmRelations(list);
               // SetMtmRelations(list);
                return list;
            }
        }
        private IEntity ReadObject(XmlReader reader, DataObjectDescription od)
        {
            var e = (EntityBase)_ps.CreateEntity(od.DataTable.Mapping.Type);//od.CreateObject();
            e.Source = _dataSource;
            e.BeginLoad();

            if (reader.GetAttribute("name") == od.DataTable.Name)
            {
                var mapping = od.DataTable.Mapping;
                e.ID = ReadId(reader.GetAttribute("id"));
                do
                {
                    reader.Read();
                    SkipWhiteSpace(reader);
                    if (reader.GetAttribute("name") == od.DataTable.Name &&
                        reader.NodeType == XmlNodeType.EndElement)
                        break;// The entity have been read
                    var propName = reader.GetAttribute("name");
                    if (mapping.Mapping.ContainsKey(propName))
                    {
                       var property = od.DataTable.Properties[propName];
                       if (property.IsOneToOne)
                       {
                           var refOd = new DataObjectDescription(_dataSource.Metadata.ExportedTypes[property.TypeId].Object.ClrType, _dataSource.GetTable(_dataSource.Metadata.ExportedTypes[property.TypeId]));
                           var targetType = _dataSource.Metadata.ExportedTypes[property.TypeId];
                           reader.Read();
                           var val = _dataSource.GetLoader(targetType).LoadEntityById(reader.Value);
                           var ed = new EntityDescription(refOd);
                           var prop = ed.EntityAttributes[property.Name];
                           prop.SetValue(e, val);
                           e.SourceRawValues.Add(property.Name, val);
                          /* DataObjectDescription refOd = new DataObjectDescription(_dataSource.Metadata.ExportedTypes[property.TypeId], _dataSource.Metadata.SourceTables[mapping.Mapping[property.Name]]);
                           using (var refReader = _sourceReader.ReadSubtree())
                           {
                               do
                               {
                                   refReader.Read();
                                   if (refReader.GetClrAttribute("name") == mapping.Mapping[property.Name] && refReader.GetClrAttribute("id") == reader.Value)
                                   {
                                       IEntity val = ReadObject(refReader, refOd);
                                       EntityDescription ed = new EntityDescription(refOd);
                                       var prop = ed.EntityAttributes[property.Name];
                                       prop.SetValue(e, val);
                                       e.SourceRawValues.Add(property.Name, val);
                                   }
                               } while (refReader.EOF); 
                           }*/
                       }
                       else if (property.IsOneToMany)
                       {
                   
                       }
                       else if (property.IsManyToMany)
                       {
                        
                       }
                       else
                       {
                           reader.Read();
                           var value = Convert.ChangeType(reader.Value, _dataSource.Metadata.ExportedTypes[property.TypeId].Object.ClrType);
                          // var prop = od.Properties[property.Name]; // Сделать возможность получения по индексу(возможно, из маппера)
                          // prop.SetValue(e, value);
                           var ed = new EntityDescription(od);
                           ed.Init();
                           var prop = ed.EntityAttributes[property.Name];
                           prop.SetValue(e, value);
                           e.SourceRawValues.Add(property.Name, value);
                       }
                    }
                    reader.Read();
                } while (reader.NodeType != XmlNodeType.EndElement);
            }
            e.EndLoad();
            return e;
        }

   
        protected IEntity ReadObject(XmlReader reader, EntityDescription od)
		{
            var e = (EntityBase)_ps.CreateEntity(od.Object.ClrType);//od.CreateObject();
		    e.Source = _dataSource;
			e.BeginLoad();

			if (reader.Name == od.Object.ClrType.Name)
			{
				e.ID = ReadId(reader.GetAttribute("Id"));
			    reader.Read();
				do
				{
					SkipWhiteSpace(reader);
					if (reader.Name == Mapper.TableName &&
						reader.NodeType == XmlNodeType.EndElement)
						break;// The entity have been read
					var propName = reader.Name;
					if (od == Mapper.GetByDbName(propName)[0].Domain) //   != null)
					{
					    var property = Mapper.GetByDbName(propName)[0];//od.EntityAttributes[];
						if (property.IsNonPersistent)
						{
							reader.Read();
							continue;
						}
                        if (property.IsMapped || property.IsManyToManyRelation)
                        {
                            if (!property.IsOneToManyRelation)
                            {
                                object value = ReadValue(property, reader);
                                if (!property.IsRelation)
                                    //{
                                    //    IList defaultPropertyValue = e[property.Name] as IList;
                                    //    foreach (var element in value as IList)
                                    //        defaultPropertyValue.AddProperty(element);
                                    //}
                                    //else
                                    //e[property.Name] = value;
                                    property.SetValue(e, value);
                                e.SourceRawValues.Add(propName, value);
                            }
                        }
                        else
                            reader.ReadEndElement();
						//od.SetReadPropertyValue(propName, e, value);
					}
				} while (!(reader.Name == Mapper.TableName && reader.NodeType == XmlNodeType.EndElement));
			}
			//}
			e.EndLoad();
			return e;
		}

        private object ReadValue(string propName, XmlReader reader, Type type)
        {
            throw new NotImplementedException();
        }


		private object ReadValue(EntityAttributeDescription property, XmlReader reader)
		{
			{
				object o = null;
				try
				{
                    //if (property.PropertyType.Name == "Byte[]")
                    //{
                    //    object b = reader.ReadElementContentAs(typeof(byte[]), null);
                    //    MemoryStream mem = new MemoryStream(b as byte[]);
                    //    o = mem.ToArray();
                    //}
                    //else if (property.PropertyType.IsEnum)
                    //{
                    //    int i = reader.ReadElementContentAsInt();
                    //    o = i;
                    //}
                    if (property.IsOneToOneRelation)
                    {
                        //o = ReadId(reader.ReadElementContentAsString());
                        o = ReadValue(property.Range.Properties[property.RelationInfo.RelatedColumn].PropertyType, reader);
                    }
                    else if (property.IsOneToManyRelation)
                    {
                        // can't read, in fact.
                        // one-to-many is computable
                    }
                    else if (property.IsManyToManyRelation)
                    {
                        o = ReadMtmValues(property, reader);
                    }
                    else
                    {
                        //string s = reader.ReadElementContentAsString();
                        //o = Convert.ChangeType(s, property.PropertyType);
                        o = ReadValue(property.Range.ClrType, reader);
                    }
				}
				catch (Exception e)
				{
					throw new Exception("Unable to read value", e);
					o = null;
				}
			    reader.Read();
				return o;
			}
		}

        private object ReadValue(Type asType, XmlReader reader)
        {
            if (asType == typeof(string))
            {
                return reader.ReadElementContentAsString();
            }
            else if (asType == typeof(Int16))
            {
                return Int16.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(Int32))
            {
                return Int32.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(Int64))
            {
                return Int64.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(UInt16))
            {
                return UInt16.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(UInt32))
            {
                return UInt32.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(UInt64))
            {
                return UInt64.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(float))
            {
                return Int16.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(double))
            {
                return double.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(decimal))
            {
                return decimal.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(DateTime))
            {
                return DateTime.FromBinary(long.Parse(reader.ReadElementContentAsString()));
            }
            else if (asType == typeof(bool))
            {
                return bool.Parse(reader.ReadElementContentAsString());
            }
            else if (asType == typeof(Guid))
            {
                return new Guid(reader.ReadElementContentAsString());
            }
            else if (asType.Name == "Byte[]")
            {
                throw new NotImplementedException("Reading byte[] values is not implemented");
                //object b = reader.ReadElementContentAs(typeof(byte[]), null);
                //var mem = new MemoryStream(b as byte[]);
                //return mem.ToArray();
            }
            else if (asType.IsEnum)
            {
                return reader.ReadElementContentAsInt();
            }
            else if (asType == typeof(object))
            {
                if (!string.IsNullOrEmpty(reader.GetAttribute("IsNull")) && reader.GetAttribute("IsNull").ToLower() == "true")
                {
                    reader.ReadElementContentAsString();
                    return null;
                }
                var text = reader.ReadElementContentAsString();
                {
                    int res;
                    if (int.TryParse(text, out res))
                        return res;
                }
                {
                    long res;
                    if (long.TryParse(text, out res))
                        return res;
                }
                {
                    try
                    {
                        return new Guid(text);
                    }
                    catch (Exception)
                    {
                    }
                }
                {
                    bool res;
                    if (bool.TryParse(text, out res))
                        return res;
                }
                return text;
            }
            else
            {
                // addd support for serializable objets
                throw new NotImplementedException();
            }
        }

	    private object ReadMtmValues(EntityAttributeDescription property, XmlReader reader)
		{
			var a = new ArrayList();
            if (reader.ReadToDescendant("Item"))
                do
                {
                    SkipWhiteSpace(reader);
                    string s = reader.ReadString();
                    if (string.IsNullOrEmpty(s))
                        continue;
                    a.Add(ReadId(s));
                } while (reader.ReadToNextSibling("Item") && reader.NodeType != XmlNodeType.EndElement);
            reader.Read();
			return a;
		}
		
		private void SkipWhiteSpace(XmlReader reader)
		{
			while(reader.NodeType != XmlNodeType.Element)
			{
				reader.Read();
			} 
		}

		private object ReadId(string id)
		{
            if (string.IsNullOrEmpty(id))
                return null;
			int i = 0;
			if (int.TryParse(id, out i))
				return i;
		    try
		    {
                var guid = new Guid(id);
                return guid;
            }
		    catch (Exception)
		    {
		        return id;
		    }
		}

        public void SetOtoRelations(List<IEntity> entities)
		{
			var props = new List<EntityAttributeDescription>();
            foreach (var p in ReflectedType.EntityAttributes.Relationships.OneToOne)
			{
				if (!p.IsNonPersistent && !p.IsInternal)
					props.Add(p);
			}
			foreach (EntityBase e in entities)
			{
				e.BeginLoad();
				foreach (var p in props)
				{
                    if (e.SourceRawValues[Mapper[p.Name]] == null)
                        //e[p.Name] = null;
                        p.SetValue(e, null);
                    else
                    {
                        var value = _ps.GetEntityById(p.Range.ClrType, e.SourceRawValues[Mapper[p.Name]]);
                        if (value == null)
                            throw new Exception(string.Format("Entity not found (type = '{0}', id = '{1}')", p.Range.ClrType, e.SourceRawValues[Mapper[p.Name]]));
                        //e[p.Name] = _ps.GetEntityById(p.RelatedType, e.SourceRawValues[Mapper[p.Name]]);
                        p.SetValue(e, value);
                    }
				}
				e.EndLoad();
			}
		}

        public void SetOtmRelations(List<IEntity> entities)
		{
			var props = new List<EntityAttributeDescription>();
            foreach (var p in ReflectedType.EntityAttributes.Relationships.OneToMany)
			{
				if (!p.IsNonPersistent && !p.IsInternal)
				{
					//if (!_ps.IsLoaded(p.RelatedType))
						_ps.GetEntities(p.Range.ClrType);
					props.Add(p);
				}
			}

			//SqlDataReader reader;
			foreach (EntityBase entity in entities)
			{
				entity.BeginLoad();
				foreach (var p in props)
				{
                    var od = EntityDescription.GetEntityDescription(p.Range);
					var pd = od.EntityAttributes[p.RelationInfo.RelatedColumn];
                    IList list;
                    //TODO: Переписать это месиво
                    if (p.Range.ClrType.IsGenericType &&
                        p.Range.ClrType == typeof(List<>).MakeGenericType(p.Range.ClrType.GetGenericArguments()[0]))
                        list = (IList)p.Range.ClrType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                    else
                        list = (IList)Activator.CreateInstance(p.Range.ClrType, this, p);

                    if (_ps.Caches[pd.Domain].ContainsElements(pd.Name, entity.ID))
                        foreach (EntityBase e in _ps.Caches[pd.Domain].GetEntitiesByRawFieldValues(pd.Name, entity.ID))
                            list.Add(e);
				    p.SetValue(entity, list);
				}
				entity.EndLoad();
			}
		}

        public void SetMtmRelations(List<IEntity> entities)
		{
			foreach (EntityBase e in entities)
			{
				var l = new List<EntityAttributeDescription>();
                foreach (var p in ReflectedType.EntityAttributes)
				{
					if (!p.IsManyToManyRelation)
						continue;
					l.Add(p);
				}
				e.BeginLoad();
				foreach (var p in l)
				{
                    IList list;
                    if (p.Range.ClrType.IsGenericType &&
                        p.Range.ClrType == typeof(List<>).MakeGenericType(p.Range.ClrType.GetGenericArguments()[0]))
                        list = (IList)p.Range.ClrType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                    else
                        list = (IList)Activator.CreateInstance(p.Range.ClrType, this, p);
                    foreach (object id in e.SourceRawValues[_mapper[p.Name]] as IList)
                    {
                        var entity = _ps.GetEntityById(p.Range.ClrType, id);
                        if (entity == null)
                            throw new Exception(string.Format("Cannot find a value of type '{1}' with id '{0}'", id,
                                                              p.Range.ClrType));
                        list.Add(entity);
                        //(e[p.Name] as IList).AddProperty(entity);
                    }
                    p.SetValue(e, list);
				}				
				e.EndLoad();
			}
		}

        public IEntity LoadEntityById(object Id)
		{
			throw new Exception("The method or operation is not implemented.");
		}

        public void ReloadEntity(IEntity entity)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#region ILoader Members


		List<T> ILoader.LoadAll<T>()
		{
            return LoadAll().ConvertAll(delegate(IEntity e)
            {
                return (T)e;
            });
		}

        public List<IEntity> LoadByFieldValue(string field, object value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}

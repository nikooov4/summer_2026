using System;
using System.Collections.Generic;
using System.Collections;
using System.Data.SqlClient;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Sql
{
	public class MsSqlDataLoader : ILoader
	{
		//private readonly SqlConnection _connection;
		readonly string _connectionString = string.Empty;
        public List<EntityDescription> _loaded = new List<EntityDescription>();
		private readonly PersistentStorage _ps;
		private readonly EntityDescription _entityDescription;
		private readonly Mapper _mapper;
		public DateTime _dt;
		private string _selectAll;
		private string _selectById;
		private string _idParamaterName = "@@id";

		public MsSqlDataLoader(PersistentStorage ps, string connectionString, Mapper mapper)
		{
            _mapper = mapper;
            _entityDescription = mapper.MappedEntity;
			_connectionString = connectionString;
			_ps = ps;
			//_connection = new SqlConnection(connectionString);
		}

		//public SqlConnection Connection
		//{
		//    get
		//    {
		//        //return new SqlConnection(_connectionString);
		//        return _connection;
		//    }
		//}

		public string ConnectionString
		{
			get { return _connectionString; }
		}

		public PersistentStorage Ps
		{
			get { return _ps; }
		}
		
		private string BuildSqlSelect(EntityDescription entityDescription)
		{
			return BuildSqlSelect(entityDescription, "");
		}

		private string BuildSqlSelect(EntityDescription entityDescription, string whereString)
		{
		    var select = "SELECT ";
			foreach (var property in entityDescription.EntityAttributes /*od.MappedProperties*/)
			{
				//if ( property.IsRelation || property.IsNonPersistent || property.IsInternal )
				//    continue;
				if (select != "SELECT ")
					select += ", ";
				select += Mapper[property, true];
			}
			select += " FROM ";
			select += Mapper.FullTableName;

			string where = " WHERE ";
			TableAttribute attr = entityDescription.DbTableAttribute;
			if (attr != null && attr.ToString() != "")
				where += "( " + attr.ToString() + " )";
			if ( whereString != "" )
			{
				if(attr != null && attr.ToString() != "")
					where += " AND ( " + whereString + " )";
				else
					where += whereString;

			}
			if (where.Trim() != "WHERE")
				select += where;

			return select;
		}

		private string BuildSqlSelect(EntityDescription entityDescription, object id)
		{
			if ( _selectById == null )
			{
                foreach (GetByIdCommandAttribute attr in entityDescription.Object.ClrAttributes.GetAttributes<GetByIdCommandAttribute>())
				{
					_selectById = attr.GetCommand();
					_idParamaterName = attr.GetIdParameterName();
				}
				if ( _selectById == null )
				{
					string where = "";
				    where += Mapper[entityDescription.SystemId, true] + " = ";
					where += _idParamaterName;
					_selectById = BuildSqlSelect(entityDescription, where);
				}
			}
			return _selectById.Replace(_idParamaterName, IdToString(id));
		}

		private string BuildGetMTMQuery(EntityAttributeDescription property)
		{
			return BuildGetMTMQuery(property, null);
		}

        private string BuildGetMTMQuery(EntityAttributeDescription property, object entityId)
		{
			var m = new Mapper(property.Domain, null);
			var mtmTableName = "";
			if ( !string.IsNullOrEmpty(property.RelationInfo.SchemaName) )
				mtmTableName = property.RelationInfo.SchemaName + "."
					+ property.RelationInfo.MamyToManyRelationTable;
			else if ( m.SchemaName != "" )
				mtmTableName = m.SchemaName + "." + property.RelationInfo.MamyToManyRelationTable;
			else
				mtmTableName = property.RelationInfo.MamyToManyRelationTable;
			var select = "SELECT ";
			select += property.RelationInfo.MtmRelationTableParentColumn + ", ";
			select += property.RelationInfo.MtmRelationTableChildColumn + " ";
			select += "FROM " + mtmTableName + " AS t1 ";

            var attr = ReflectedType.DbTableAttribute;
			if ( attr != null && attr.ToString() != "" )
			{
				var select2 = "INNER JOIN " + Mapper.FullTableName + " AS t2 ON ";
				select2 += "t1." + property.RelationInfo.MtmRelationTableParentColumn + " = ";
                select2 += "t2." + Mapper[ReflectedType.SystemId, true];
				select2 += " AND t2." + attr.ToString();
				select += select2;
			}
			if ( entityId != null )
			{
				var where = " WHERE t1." + m[ReflectedType.SystemId, true] + " = " + IdToString(entityId);
				select += where;
			}
			return select;
		}

		private string IdToString(object id)
		{
			if ( id is Guid || id is string )
				return "'" + id.ToString() + "'";
			return id.ToString();
		}

		private void GetReader(string query, List<object[]> values)
		{
			
			using (var connection = new SqlConnection(_connectionString))
			{

				using (var command = new SqlCommand(query, connection))
				{
					
					connection.Open();
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							object[] vals = new object[reader.FieldCount];
							reader.GetValues(vals);
							values.Add(vals);
						}
					}
				}
			}
			
		}

        public List<T> LoadAll<T>() where T : IEntity
		{
			var list = LoadAll();
			List<T> result = list.ConvertAll(
                delegate(IEntity e)
				{
					return (T)e;
				});
			return result;
		}

        public List<IEntity> LoadAll()
		{
            string select = null;
			if ( _selectAll == null )
				_selectAll = select = BuildSqlSelect(ReflectedType);
			else
				select = _selectAll;
			//DateTime t1 = _dt = DateTime.Now;
            List<IEntity> list = FillEntityList(select);
			if (list.Count > 0)
			{
				SetOtoRelations(list);
				SetOtmRelations(list);
				SetMtmRelations(list);
			}
			//}
			foreach ( EntityBase entity in list )
				entity.EndLoad();
			return list;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader">Will be closed</param>
		/// <returns></returns>
        private List<IEntity> FillEntityList(string select)
		{
		    var list = new List<IEntity>();
			//int i = 0;
			//DateTime t21 = _dt = DateTime.Now;
			
			using (var _connection = new SqlConnection(_connectionString))
			{

				using (var command = new SqlCommand(select, _connection))
				{
					_connection.Open();
					using (SqlDataReader reader = command.ExecuteReader())
					{
                        if (reader != null && reader.HasRows)
                        {
                            while(reader.Read())
                            {
                                //i++;
                                var entity = (EntityBase) ReflectedType.CreateObject();
                                entity.BeginLoad();
                                for (int fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
                                {
                                    var dbName = reader.GetName(fieldIndex);
                                    var entityAttribute = Mapper.GetByDbName(dbName)[0];
                                    var o = reader.GetValue(fieldIndex);
                                    //if (od.EntityAttributes.Contains(name) && od.EntityAttributes[name].IsMapped)
                                    entity[entityAttribute] = o;
                                    entity.SourceRawValues.Add(dbName, o);
                                }
                                list.Add(entity);

                                if (!Ps.Caches[ReflectedType].Contains(entity.ID))
                                    Ps.Caches[ReflectedType].Add(entity);
                                else
                                    Ps.Caches[ReflectedType][entity.ID] = entity;
                                
                            }
                        }
					}
					_loaded.Add(ReflectedType);
					
				}
			}
			//DateTime t22 = DateTime.Now;
			//TimeSpan ts2 = t22 - t21;
			return list;
		}

        public IEntity LoadEntityById(object id)
		{
			string select = BuildSqlSelect(_entityDescription, id);
            List<IEntity> list = FillEntityList(select);
			if (list.Count > 1)
				throw new Exception("LoadEntityById: query returned too much entities.");
			if (list.Count < 1)
				return null;
			//    throw new Exception("LoadEntityById: query return am empty result.");
			SetOtoRelations(list[0]);
			SetOtmRelations(list[0]);
			SetMtmRelations(list[0]);
			foreach ( EntityBase entity in list )
				entity.EndLoad();
			return list[0];
		}

        public List<IEntity> LoadByFieldValue(string field, object value)
		{
			var select = BuildSqlSelect(_entityDescription, _mapper[field, true] + " = " + IdToString(value));
			SqlDataReader reader = null;
            List<IEntity> list = FillEntityList(select);
			
			if (list.Count > 0)
			{
				SetOtoRelations(list);
				SetOtmRelations(list);
				SetMtmRelations(list);
			}
			
			return list;
		}

        //public List<List<IEntity>> LoadAllRelatedTypes()
        //{
        //    var od = _ps.EntitiesFactory.GetObjectDescription(ReflectedEntityDescription);
        //    var list = new List<List<IEntity>>();
        //    foreach ( PropertyDescription p in od.Relations )
        //    {
        //        var l = Ps.GetEntities(p.RelatedType);////
        //        list.AddProperty(l);
        //    }
        //    return list;
        //}

		//public List<List<EntityBase>> LoadAllRelatedTypesRec(Type entityDescription)
		//{
		//    List<List<EntityBase>> list = new List<List<EntityBase>>();
		//    EntityDescription od = new EntityDescription(entityDescription);
		//    foreach ( PropertyDescription prop in od.Relations )
		//    {
		//        bool load = true;
		//        foreach ( Type t in _loaded )
		//            if ( t == prop.RelatedType )
		//            {
		//                load = false;
		//                break;
		//            }
		//        if ( load )
		//        {
		//            LoadAll(prop.RelatedType);
		//            LoadAllRelatedTypesRec(prop.RelatedType);
		//        }
		//    }
		//    return list;
		//}

        public void SetOtoRelations(List<IEntity> entities)
		{
			var props = new List<EntityAttributeDescription>();
            foreach (var p in ReflectedType.EntityAttributes.Relationships)
			{
				if (p.IsOneToOneRelation && !p.IsNonPersistent && !p.IsInternal)
				{
					//if (!Ps.IsLoaded(p.RelatedType))
						Ps.GetEntities(p.Range.ClrType);
					props.Add(p);
				}
			}
            foreach (IEntity e in entities)
			{
				foreach ( var p in props )
				{
					if ( e.Description.SourceRawValues[Mapper[p.Name]] != null )
					{
						if ( e.Description.SourceRawValues[Mapper[p.Name]] is Guid && (Guid)e.Description.SourceRawValues[Mapper[p.Name]] != Guid.Empty )
							e[p.Name] = Ps.GetEntityById(p.Range.ClrType, e.Description.SourceRawValues[Mapper[p.Name]]);
						else
							e[p.Name] = null;
					}
				}
			}
		}

        public void SetOtmRelations(List<IEntity> entities)
		{
			var props = new List<EntityAttributeDescription>();
            foreach (var p in ReflectedType.EntityAttributes.Relationships)
			{
				if (p.IsOneToManyRelation && !p.IsNonPersistent && !p.IsInternal)
				{
					//if (!Ps.IsLoaded(p.RelatedType))
						Ps.GetEntities(p.Range.ClrType);
					props.Add(p);
				}
			}
			
			//SqlDataReader reader;
            foreach (var entity in entities)
			{
				foreach ( var p in props )
				{
                    var od = _ps.EntitiesFactory.GetObjectDescription(p.Range.ClrType);
					var pd = od.EntityAttributes[p.RelationInfo.RelatedColumn];
					//Indexes indexes = Ps.Caches[p.RelatedType].RawIndexes;
					//Mephi.Cybernetics.Race.DataAccess.Mapper m = new Mapper(p.RelatedType, _ps);
					//List<EntityBase> list = null;
					//if ( indexes[m[pd.Name]].ContainsKey(entity.ID) )
					//    list = indexes[m[pd.Name]][entity.ID];
					//if ( list != null )
					//    foreach ( EntityBase e in list )
					//        (p.GetValue(entity) as IList).AddProperty(e);
					if ( _ps.Caches[pd.Domain].ContainsElements(pd.Name, entity.ID) )
						foreach ( EntityBase e in _ps.Caches[pd.Domain].GetEntitiesByRawFieldValues(pd.Name, entity.ID) )
							(p.GetValue(entity) as IList).Add(e);
				}
			}
		}

        public void SetMtmRelations(List<IEntity> entities)
		{
            foreach (var p in _mapper.MappedEntity.EntityAttributes.Relationships.ManyToMany)
			{
				if (!p.IsNonPersistent && !p.IsInternal)
				{
					//
					//if (!Ps.IsLoaded(p.RelatedType))
						Ps.GetEntities(p.Range.ClrType);

					//DateTime dt1 = DateTime.Now;
					var list = new List<object[]>();
					GetReader(BuildGetMTMQuery(p), list);




					foreach (object[] val in list)
					{
						//DateTime dt3 = DateTime.Now;
						//int i = 0;

						//DateTime dt7 = DateTime.Now;
						var parentEntity = Ps.GetEntityById(p.Domain.Object.ClrType, val[0]);
						var childEntity = Ps.GetEntityById(p.Range.ClrType, val[1]);
						//DateTime dt8 = DateTime.Now;
						//TimeSpan ts3 = dt8 - dt7;
						//(parentEntity[p.Name] as IList).AddProperty(childEntity);
						(p.GetValue(parentEntity) as IList).Add(childEntity);
						//i++;
					}
					//DateTime dt2 = DateTime.Now;
					//string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
					//path = System.IO.Path.GetDirectoryName(path);
					//path = System.IO.Path.Combine(path, "log.txt");
					//System.IO.StreamWriter w = new System.IO.StreamWriter(path, true);
					//w.WriteLine();
					//w.Write("Type name: ");
					//w.Write(p.ReflectedEntity.ClrType.Name);
					//w.Write(", property name: ");
					//w.Write(p.Name);
					//w.Write(", count: ");
					//w.Write(i);
					//w.Write(", estimated time: ");
					//w.WriteLine(dt2 - dt3);
					//w.Flush();
					//w.Close();
					//TimeSpan ts1 = dt2 - dt1;
					//TimeSpan ts2 = dt2 - dt3;
				}
			}
		}

        public void SetAllRelations(List<IEntity> entities)
		{
			throw new NotImplementedException();
		}

        public void SetOtoRelations(IEntity entity)
		{
            foreach (var p in ReflectedType.EntityAttributes.Relationships.OneToOne)
			{
				if ( !p.IsNonPersistent && !p.IsInternal )
					if ( entity.Description.SourceRawValues[Mapper[p.Name]] != null )
					if ( entity.Description.SourceRawValues[Mapper[p.Name]] is Guid && (Guid)entity.Description.SourceRawValues[Mapper[p.Name]] != Guid.Empty )
					entity[p.Name] = Ps.GetEntityById(p.Range.ClrType, entity.Description.SourceRawValues[Mapper[p.Name]]);
			}
		}

        public void SetOtmRelations(IEntity entity)
		{
            foreach (var p in _mapper.MappedEntity.EntityAttributes.Relationships.OneToMany)
			{
				if ( !p.IsNonPersistent && !p.IsInternal )
				{
                    var od = _ps.EntitiesFactory.GetObjectDescription(p.Range.ClrType);
					var pd = od.EntityAttributes[p.RelationInfo.RelatedColumn];
					var m = new Mapper(pd.RelatedEntity, null);
					string select = "SELECT " + m[pd, true] + " FROM " + m.FullTableName + " AS t" +
						" WHERE " + m[pd, true] + " = " + IdToString(entity.ID);
					var list = new List<object[]>();
					GetReader(select, list);

					foreach (object[] val in list)
					{
						var id = val[0];
						var e = _ps.GetEntityById(p.Range.ClrType, id);
						if (e != null)
							(p.GetValue(entity) as IList).Add(e);

					}


				}
			}
		}

        public void SetMtmRelations(IEntity entity)
		{
            foreach (var p in _mapper.MappedEntity.EntityAttributes.Relationships.ManyToMany)
				if ( !p.IsNonPersistent && !p.IsInternal )
			{
				
				//if ( !Ps.IsLoaded(p.RelatedType) )
					Ps.GetEntities(p.Range.ClrType);

				var list = new List<object[]>();
				GetReader(BuildGetMTMQuery(p, entity.ID), list);
				foreach (object[] val in list)
				{
					var parentEntity = Ps.GetEntityById(p.Domain.Object.ClrType, val[0]);
					var childEntity = Ps.GetEntityById(p.Range.ClrType, val[1]);
					(p.GetValue(parentEntity) as IList).Add(childEntity);
				}
			}
		}

        public void SetAllRelations(IEntity entity)
		{
			throw new NotImplementedException();
		}


		#region ILoader Members

		public EntityDescription ReflectedType
		{
			get { return _entityDescription; }
		}

		public Mapper Mapper
		{
			get { return _mapper; }
		}

		//public List<EntityBase> LoadAll()
		//{
		//    throw new Exception("The method or operation is not implemented.");
		//}

		//public IEntity LoadEntityById(object Id)
		//{
		//    throw new Exception("The method or operation is not implemented.");
		//}

        public void ReloadEntity(IEntity entity)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}

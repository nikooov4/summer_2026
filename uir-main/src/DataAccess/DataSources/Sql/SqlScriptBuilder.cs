using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Sql
{
	/// <summary>
	/// Description of SqlScriptBuilder.
	/// </summary>
	public class SqlScriptBuilder
	{
		private readonly Mapper _mapper;
		private readonly PersistentStorage _ps;
		private readonly string _insertTemplate = "INSERT INTO @@TableName ( @@FieldList ) @@Values";
		private readonly string _updateTemplate = "UPDATE @@TableName SET @@NameValuePairs WHERE ID = @@ID";
		private readonly string _updateInBatchTemplate = "UPDATE @@TableName SET @@NameValuePairs FROM (@@ValuesTable) AS temp WHERE @@TableName.ID = temp.ID";
		private readonly string _deleteTemplate = "DELETE FROM @@TableName WHERE ID IN ( @@ID )";

		public SqlScriptBuilder(Mapper mapper, PersistentStorage ps)
		{
			_ps = ps;
			_mapper = mapper;
			_insertTemplate = _insertTemplate.Replace("@@TableName", mapper.FullTableName);
			_updateTemplate = _updateTemplate.Replace("@@TableName", mapper.FullTableName);
			_updateInBatchTemplate = _updateInBatchTemplate.Replace("@@TableName", mapper.FullTableName);
			_deleteTemplate = _deleteTemplate.Replace("@@TableName", mapper.FullTableName);
		}

		private static string ValueToString(object value)
		{
			if ( value == null )
				return "NULL";
			if ( value is UnasignedValue )
				return "NULL";
			if (  value is string )
				return "'" + (value as string).Replace("'", "''") + "'";
			if ( value is bool )
				return ((bool)value) ? "1" : "0";
			if ( value is Guid )
				return "'" + value.ToString() + "'";
			if ( value is DateTime )
				return "'" + ((DateTime)value).ToString("u").Replace("Z", "") + "'";
			if ( value is char )
				return ((char)value).ToString();
			if ( value as EntityBase != null )
				return ValueToString((value as EntityBase).ID);
			if ( value.GetType().IsEnum )
				return Convert.ToInt32(value).ToString();
			return value.ToString();
		}
		
		public PersistentStorage Ps
		{
			get
			{
				return _ps;
			}
		}

		public Mapper SqlMapper
		{
			get
			{
				return _mapper;
			}
		}
		
		public string CreateUpdateStatement(EntityBase e)
		{
		    var nameValuePairs = "";
            foreach (var pd in _mapper.MappedEntity.EntityAttributes)
			{
				if ( pd == _mapper.MappedEntity.SystemId || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				if ( nameValuePairs != "" )
					nameValuePairs += ", ";
				if ( pd.IsOneToOneRelation )
					nameValuePairs += SqlMapper[pd.Name] + " = " + (e[pd.Name] as EntityBase)[pd.RelationInfo.RelatedColumn];
				else
					nameValuePairs += SqlMapper[pd.Name] + " = " + ValueToString(e[pd.Name]);
			}
            nameValuePairs += ", " + ValueToString(e.ID) + " AS " + _mapper[_mapper.MappedEntity.SystemId.Name];
			return _updateTemplate.Replace("@@NameValuePairs", nameValuePairs).Replace("@@ID", ValueToString(e.ID));
		}
		
		private string GetFieldList()
		{
			string list = "";
		    foreach (var pd in _mapper.MappedEntity.EntityAttributes)
			{
				if ( pd == _mapper.MappedEntity.SystemId || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				if ( list != "" )
					list += ", ";
                list += SqlMapper[pd, true];
			}
			return list;
		}
		
		private string GetValueList(EntityBase e)
		{
			string list = "";
		    foreach (var pd in _mapper.MappedEntity.EntityAttributes)
			{
				if ( pd == _mapper.MappedEntity.SystemId || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				if ( list != "" )
					list += ", ";
				if ( pd.IsOneToOneRelation )
					list += (e[pd.Name] as EntityBase)[pd.RelationInfo.RelatedColumn];
				else
					list += ValueToString(e[pd.Name]);
			}
			return list;
		}

		public string CreateUpdateStatement(IList<EntityBase> list)
		{
			string fieldList = "";
			string nameValuePairs = "";
		    foreach (var pd in _mapper.MappedEntity.EntityAttributes)
			{
				if ( pd == _mapper.MappedEntity.SystemId || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				if ( fieldList != "" )
					fieldList += ", ";
				fieldList += SqlMapper[pd.Name];
				
				if ( nameValuePairs != "" )
					nameValuePairs += ", ";
                nameValuePairs += _mapper[pd, true] + " = temp." + _mapper[pd, true];
			}
			string valuesTable = "";
			foreach ( EntityBase e in list)
			{
				string valuesRow = "";
                foreach (var pd in _mapper.MappedEntity.EntityAttributes)
				{
					if ( pd == _mapper.MappedEntity.SystemId || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
						continue;
					if ( valuesRow != "" )
						valuesRow += ", ";
                    valuesRow += ValueToString(e[pd.Name]) + " AS " + _mapper[pd.Name];
				}
				valuesRow += ", " + ValueToString(e.ID) + " AS " + _mapper[_mapper.MappedEntity.SystemId.Name];
				valuesRow = "SELECT " + valuesRow;
				if ( valuesTable != "" )
					valuesTable += "\r\nUNION ALL\r\n";
				valuesTable += valuesRow;
			}
			return _updateInBatchTemplate.Replace("@@FieldList", fieldList).Replace("@@NameValuePairs", nameValuePairs).Replace("@@ValuesTable", valuesTable);
		}

		public string CreateDeleteStatement(EntityBase e)
		{
			return CreateDeleteStatement(new List<EntityBase>( new EntityBase[] { e } ));
		}

		public string CreateDeleteStatement(IList<EntityBase> list)
		{
			string ids = "";
			foreach ( EntityBase e in list )
			{
				if ( ids != "" )
					ids += ", ";
				ids += ValueToString(e.ID);
			}
			return _deleteTemplate.Replace("@@ID", ids);
		}

		public string CreateInsertStatement(EntityBase e)
		{
			return CreateInsertStatement(new List<EntityBase>( new EntityBase[] { e } ));
		}

		public string CreateInsertStatement(IList<EntityBase> list)
		{
			var fieldList = "";
		    fieldList += _mapper[_mapper.MappedEntity.SystemId, true];
            foreach (var pd in _mapper.MappedEntity.EntityAttributes)
			{
				if ( pd == _mapper.MappedEntity.SystemId || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				//if ( fieldList != "" )
				fieldList += ", ";
				fieldList += SqlMapper[pd.Name];
			}
			var valuesTable = "";
			foreach ( var e in list)
			{
				var valuesRow = "";
				valuesRow += ValueToString(e.ID);
                foreach (var pd in _mapper.MappedEntity.EntityAttributes)
				{
					if ( pd == _mapper.MappedEntity.SystemId || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
						continue;
					//if ( valuesRow != "" )
					valuesRow += ", ";
					valuesRow += ValueToString(e[pd.Name]);
				}
				valuesRow = "SELECT " + valuesRow;
				if ( valuesTable != "" )
					valuesTable += "\r\nUNION ALL\r\n";
				valuesTable += valuesRow;
			}
			return _insertTemplate.Replace("@@FieldList", fieldList).Replace("@@Values", valuesTable);
		}

        public static string CreateMtmInsert(EntityAttributeDescription pd, PersistentStorage ps, IList<EntityBase> entities)
		{
			var attr = pd.ObjectProperty.GetAttribute<RelationAttribute>();
			if ( attr == null || attr.RelationType != RelationType.ManyToMany )
				throw new Exception("A many-to-many relationship expected");
			string script = "INSERT INTO " + (string.IsNullOrEmpty(attr.SchemaName) ? "" : attr.SchemaName + ".") +
				attr.MamyToManyRelationTable + "(" + attr.MtmRelationTableChildColumn + "," + attr.MtmRelationTableParentColumn + ")\r\n";
			string data = "";
			foreach ( EntityBase entity in entities )
				foreach ( EntityBase e in pd.GetValue<System.Collections.IList>(entity) )
					if ( string.IsNullOrEmpty(data) )
						data = "SELECT " + ValueToString(e.ID) + ", " + ValueToString(entity.ID) + "\r\n";
					else
						data += "UNION ALL\r\nSELECT " + ValueToString(e.ID) + ", " + ValueToString(entity.ID) + "\r\n";
			if ( string.IsNullOrEmpty(data) )
				return "";
			else
				script += data;
			return script;
		}

        public static string CreateMtmDelete(EntityAttributeDescription pd, PersistentStorage ps, IList<EntityBase> entities)
		{
			var attr = pd.ObjectProperty.GetAttribute<RelationAttribute>();
			if ( attr == null || attr.RelationType != RelationType.ManyToMany )
				throw new Exception("A many-to-many relationship expected");
			string script = "DELETE FROM " + (string.IsNullOrEmpty(attr.SchemaName) ? "" : attr.SchemaName + ".") + 
				attr.MamyToManyRelationTable + "\r\nWHERE ";
			script += attr.MtmRelationTableParentColumn+" IN (";
			string data = "";
			foreach ( EntityBase entity in entities )
				if ( string.IsNullOrEmpty(data) )
					data = ValueToString(entity.ID);
				else
					data += ", " + ValueToString(entity.ID);
			script += data + ")\r\n";
			return script;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Xml;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources
{
	public class Mapper
	{
		private readonly string _tableName;
		private readonly string _schemaName = "";
        private readonly EntityFieldsMapper _entityFieldsMapper;
		private readonly EntityDescription _entityDescription;

        public Mapper(Type entityType, XmlReader xmlReader)
            : this(EntityDescription.GetEntityDescription(entityType), xmlReader)
	    {
	    }

        public Mapper(EntityDescription entityDescription, XmlReader xmlReader)
        {
            _entityDescription = entityDescription;
		    if (xmlReader != null)
		    {
		        var data = ReadEntityMapping(xmlReader);
		        _tableName = data.TableName;
		        _schemaName = data.SchemaName;
		        _entityFieldsMapper = data.EntityFieldsMapper;
		    }
		    else
		    {
		        var attr = _entityDescription.DbTableAttribute;
		        _tableName = attr == null ? _entityDescription.Name : attr.TableName;
		        var attr2 = _entityDescription.Object.GetClrAttribute<DbSchemaAttribute>();
		        if (attr2 != null)
		            _schemaName = attr2.SchemaName;
                _entityFieldsMapper = new EntityFieldsMapper(MappedEntity, null);
            }

            FullTableName = string.IsNullOrEmpty(_schemaName) ? _tableName : _schemaName + "." + _tableName;
        }

	    public EntityDescription MappedEntity => _entityDescription;

        public string SchemaName => _schemaName;

        public string TableName => _tableName;

        public string FullTableName { get; }

        public string this[string propertyName] => _entityFieldsMapper[_entityDescription.Attributes[propertyName]];

        public string this[string propertyName, bool bracketed] => this[_entityDescription.Attributes[propertyName], bracketed];

        public string this[EntityAttributeDescription property] => _entityFieldsMapper[property];

        public string this[EntityAttributeDescription property, bool bracketed]
	    {
            get
            {
                if (bracketed) 
                    return "[" + this[property] + "]";
                
                return this[property];
            }
	    }

        public List<EntityAttributeDescription> GetByDbName(string dbName)
		{
			return _entityFieldsMapper.GetByDbName(dbName);
		}

        private MapperData ReadEntityMapping(XmlReader xmlReader)
        {
            if (!xmlReader.IsStartElement())
                throw new ArgumentException("Invalid entity mapper node");
            var data = new MapperData();
            while (xmlReader.NodeType != XmlNodeType.EndElement)
            {
                xmlReader.Read();
                if (xmlReader.IsStartElement())
                {
                    switch (xmlReader.Name)
                    {
                        case "table-name":
                            data.TableName = xmlReader.GetAttribute("name");
                            xmlReader.Read();
                            break;
                        case "schema":
                            data.SchemaName = xmlReader.GetAttribute("name");
                            xmlReader.Read();
                            break;
                        case "properties":
                            data.EntityFieldsMapper = new EntityFieldsMapper(_entityDescription, xmlReader.ReadSubtree());
                            xmlReader.Read();
                            break;
                    }
                    xmlReader.ReadEndElement();
                }
            }
            return data;
        }

	    private struct MapperData
	    {
	        public string TableName;
            public EntityFieldsMapper EntityFieldsMapper;
            public string SchemaName;
        }

		private class EntityFieldsMapper
		{
            private readonly Dictionary<EntityAttributeDescription, string> _entityFieldsMapping = 
                new Dictionary<EntityAttributeDescription, string>();
            private readonly Dictionary<string, List<EntityAttributeDescription>> _entityFieldsMappingInverse = 
                new Dictionary<string, List<EntityAttributeDescription>>(new StringCaseInsensitiveComparer());

            public EntityFieldsMapper(EntityDescription entityDescription, XmlReader xmlReader)
			{
			    if (xmlReader != null)
			        ReadXmlMapping(xmlReader, entityDescription);
			    else
			    {
			        foreach (var prop in entityDescription.EntityAttributes)
			        {
			            string dbPropName;
			            var attr = prop.ObjectProperty.GetAttribute<FieldNameAttribute>();
			            if (attr == null)
			                dbPropName = prop.Name;
			            else if (prop.Domain.PrimaryKey != null && prop.Domain.PrimaryKey.Name == prop.Name)
			                dbPropName = prop.Domain.PrimaryKey.Name;
			            else
			                dbPropName = attr.FieldName;

                        AddPropertyMapping(prop, dbPropName);
			        }
			    }
			}

            public string this[EntityAttributeDescription property] => _entityFieldsMapping[property];

            public List<EntityAttributeDescription> GetByDbName(string dbName)
            {
                if (_entityFieldsMappingInverse.ContainsKey(dbName))
                    return _entityFieldsMappingInverse[dbName];
                else
                    throw new KeyNotFoundException();
            }

            private void AddPropertyMapping(EntityAttributeDescription property, string columnName)
		    {
                _entityFieldsMapping.Add(property, columnName);

                if (!_entityFieldsMappingInverse.ContainsKey(columnName))
                    _entityFieldsMappingInverse.Add(columnName, new List<EntityAttributeDescription>());
                _entityFieldsMappingInverse[columnName].Add(property);
            }

		    private void ReadXmlMapping(XmlReader xmlReader, EntityDescription od)
		    {
                if (!xmlReader.IsStartElement())
                    throw new ArgumentException("Invalid properties node");

                while (xmlReader.NodeType != XmlNodeType.EndElement)
                {
                    xmlReader.Read();
                    if (xmlReader.IsStartElement())
                    {
                        var property = od.Attributes[xmlReader.GetAttribute("name")];
                        switch (xmlReader.Name)
                        {
                            case "property":  // works for both normal attributes and OTO association
                                var columnName = xmlReader.GetAttribute("column-name");
                                AddPropertyMapping(property, columnName);
                                xmlReader.Read();
                                break;
                        }
                        xmlReader.ReadEndElement();
                    }
                }
            }

		}
	}

}

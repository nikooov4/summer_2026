using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using System.Xml.XPath;
using System.Drawing;
using System.Collections;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Xml
{
	public class XmlDataSaver : ISaver
	{
	    private readonly XmlWriter _writer;
        private readonly EntityDescription _entityEntityDescription;
		private readonly Mapper _mapper;
		private XmlDocument _document;
		private readonly XPathNavigator _rootNavigator;
        private string[] _entitiesTagPath;
        private string _fileName;
        private DataSource _dataSource;

        private XmlDataSaver(EntityDescription entityDescription, DataSource dataSource)
	    {
            _entityEntityDescription = entityDescription;
            _dataSource = dataSource;
            _mapper = dataSource.GetMapper(entityDescription);
        }
        
        //public XmlDataSaver(XmlWriter writer, PersistentStorage ps, Type entityDescription)
		//{
		//    _entityEntityDescription = entityDescription;
		//    _ps = ps;
		//    _sourceWriter = writer;
		//    _mapper = new Mapper(entityDescription);
		//}


        public XmlDataSaver(XPathNavigator navigator, EntityDescription entityDescription, DataSource dataSource) : 
            this(entityDescription, dataSource)
		{
		    _rootNavigator = navigator;
		}

	    private XPathNavigator NavigateToNode(string nodePath)
		{
			XPathNavigator navigator = _document.CreateNavigator();
			string[] path = nodePath.Replace("\\",".").Replace("/",".").Replace("->",".").Split('.');
			foreach (string s in path)
				navigator.MoveToChild(s, "");
			return navigator;
		}

        public EntityDescription ReflectedType
		{
			get
			{
				return _entityEntityDescription;
			}
		}

		public Mapper Mapper
		{
			get
			{
				return _mapper;
			}
		}
		
		public System.Data.DataSet UnderlyngDataSet
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

        public void Save(StorageCommand command)
        {
            Save(new List<StorageCommand>(new StorageCommand[] { command }));
        }

        private void ProcessComand(StorageCommand command, XPathNavigator nav)
        {
            switch (command.CommandType)
            {
                case StorageCommandType.Insert:
                    foreach (var e in command.Entities)
                        Insert(e, nav);
                    break;
                case StorageCommandType.Delete:
                    foreach (var e in command.Entities)
                        DeleteEntity(e, nav);
                    break;
                case StorageCommandType.Update:
                    foreach (var e in command.Entities)
                        Update(e, nav);
                    break;
            }
        }

        public void Save(List<StorageCommand> commands)
        {
            foreach (var command in commands)
            {
                XPathNavigator nav = _rootNavigator.CreateNavigator();
                ProcessComand(command, nav);
            }
        }

		public void Save(EntityBase entity)
		{
			XPathNavigator nav = _rootNavigator.CreateNavigator();
			if (entity.State == EntityState.Unchanged)
				return;
			else if (entity.State == EntityState.New)
				Insert(entity, nav);
			else if (entity.State == EntityState.Deleted)
				DeleteEntity(entity, nav);
			else
				Update(entity, nav);

		    //if (_navigator == null)
		    //    _navigator = NavigateToNode(_entitiesTagPath);

		    //if (entity.State == EntityState.Unchanged)
		    //    return;
		    //EntityDescription od = EntityFactory.GetObjectDescription(entity.GetType(), _ps);
		    //EntityBase ent;
		    //if (od.IsWrapped)
		    //{
		    //    ent = (entity as IWrapObject).WrappedObject;
		    //    _ps.SaveInner(ent);
		    //    return;
		    //}
		    //else
		    //    ent = entity;
		    //if (!_navigator.MoveToFollowing(GetParentNodeName(), ""))
		    //    CreateNodeContainer(_navigator);
		    //_navigator.MoveToFollowing(GetParentNodeName(), "");
		    ////foreach (EntityBase entity in entities)
		    ////{
		    //    if (ent.State == EntityState.New)
		    //        Insert(entity, _navigator);
		    //    else if (ent.State == EntityState.Deleted)
		    //        DeleteEntity(ent, _navigator);
		    //    else
		    //        Update(entity, _navigator);
		    //}
		}

		public void Save(List<EntityBase> entities)
		{
			foreach (EntityBase e in entities)
				Save(e);
			//XmlDocument doc = (_command.Connection as XmlConnection).Document;
			//XPathNavigator nav = doc.CreateNavigator();
			//if (!nav.MoveToFollowing(GetParentNodeName(), ""))
			//    CreateNodeContainer(nav);
			//nav.MoveToFollowing(GetParentNodeName(), "");
			//foreach (EntityBase entity in entities)
			//{
			//    if (entity.Id != 0)
			//        Update(entity, nav);
			//    else
			//        Insert(entity, nav);
			//}
		}

		private void DeleteEntity(EntityBase entity, XPathNavigator nav)
		{
		    if (!MoveToId(ref nav, entity.ID.ToString()))
                throw new Exception(string.Format("Could not find entity with id '{0}' if the XML document", entity.ID));
			nav.DeleteSelf();
		    nav.MoveToParent();
		}

		protected string GetParentNodeName()
		{
			return Mapper.TableName;
		}

		private void Insert(EntityBase entity, XPathNavigator nav)
		{
			//nav.MoveToFirstChild();
			if (entity.ID == null)
			{
				throw new Exception("Shit, what have I done?!");
			}
			//nav.MoveToParent();
			XmlWriter writer = nav.AppendChild();
			CreateXmlNodeEl(entity, writer);
			writer.Close();
			nav.MoveToNext();
		}
		
		private void Update(EntityBase entity, XPathNavigator nav)
		{
			bool hasChildren = true;
			if (nav.LocalName == GetParentNodeName())
				hasChildren = nav.MoveToFirstChild();
			if (MoveToId(ref nav, entity.ID.ToString()))
			{
				WriteEntity(entity, nav);
			}

			else
			{
				if (hasChildren)
					nav.MoveToParent();
				XmlWriter writer = nav.AppendChild();
				CreateXmlNodeEl(entity, writer);
				writer.Close();
				nav.MoveToNext();
			}
		}

		private void CreateXmlNodeEl(EntityBase entity, XmlWriter writer)
		{
			writer.WriteStartElement(Mapper.TableName);
			writer.WriteAttributeString("Id", entity.ID.ToString());
            foreach (var property in ReflectedType.EntityAttributes)
			{
				if (property.IsId || (!property.IsMapped && !property.IsManyToManyRelation))
					continue;


				writer.WriteStartElement(Mapper[property.Name]);
				object value = null;
				try
				{
					if (property.IsManyToManyRelation)
					{
						WriteMtm(entity, property, writer);
					}
					else
					{
                        if (property.IsOneToOneRelation)
                            value = entity[property.Name] != null ? (entity[property.Name] as EntityBase).ID : null;
                        else
                            value = entity[property.Name];
						value = PrepareToSave(value);
                        writer.WriteAttributeString("IsNull", (value == null).ToString());
						writer.WriteValue(value ?? "");
					}
				}
				catch(Exception ex)
				{
					throw new ApplicationException(string.Format("Unable to write attribute: {0}", property.Name));
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		private void WriteMtm(EntityBase e, EntityAttributeDescription p, XmlWriter w)
		{
			foreach (EntityBase entity in e[p.Name] as IList)
			{
				w.WriteStartElement("Item");
				w.WriteValue(entity.ID.ToString());
				w.WriteEndElement();
			}
		}

		private void WriteEntity(EntityBase entity, XPathNavigator nav)
		{
            foreach (var prop in ReflectedType.EntityAttributes)
            {
                if ( !prop.IsMapped )
                    continue;

                if ( prop.IsId )
                {
                    nav.MoveToAttribute("Id", "");
                    //nav.SetValue(Convert.ChangeType(entity[prop.Name], typeof(string)) as string);
                    nav.SetValue(Convert.ChangeType(PrepareToSave(entity[prop.Name]), typeof (string)) as string);
                    nav.MoveToParent();
                    continue;
                }

                if(!nav.MoveToChild(_mapper[prop.Name], ""))
                    nav.AppendChildElement("", _mapper[prop.Name], "", "");
                try
                {
                    object value = entity[prop.Name];
                    SaveValue(value, prop, nav);
                }
                catch
                {
                    throw new ApplicationException(string.Format("Unable to write attribute: {0}", prop.Name));
                }
                nav.MoveToParent();
            }
		}

		private bool MoveToId(ref XPathNavigator nav, string id)
		{
			string attr = nav.GetAttribute("Id", "");
			bool succeed = true;
			while (attr != id)
			{
				succeed = nav.MoveToFollowing(nav.LocalName, nav.NamespaceURI);
				if (!succeed)
					return false;
				attr = nav.GetAttribute("Id", "");
			}
			return true;
		}

		private object PrepareToSave(object value)
		{
			if (value == null)
				return null;
			else if (value.GetType().IsEnum)
				return (int)value;
			else if (value is Image image)
			{
				var ms = new MemoryStream();
				image.Save(ms, image.RawFormat);
				return ms.ToArray();
			}
			else if (value is byte[])
			{
                throw new NotImplementedException("byte[] serialization not implemented");
			}
            if (value is string)
            {
                return value;
            }
            else if (value is DateTime)
            {
                return ((DateTime) value).ToBinary().ToString();
            }
            else
            {
                return value.ToString();
            }
		}

        private void SaveValue(object value, EntityAttributeDescription property, XPathNavigator nav)
        {
            if (property.IsManyToManyRelation && value != null)
            {
                foreach (XPathNavigator childNode in nav.SelectChildren(XPathNodeType.Element))
                    childNode.DeleteSelf();
                foreach (EntityBase e in value as IList)
                    nav.AppendChildElement("", "Item", "", e.ID.ToString());
                return;
            }
            else if (property.IsOneToOneRelation && value != null)
            {
                value = (value as EntityBase).ID;
            }
            else
                value = PrepareToSave(value);
            if (!nav.MoveToAttribute("IsNull", ""))
                nav.CreateAttribute("", "IsNull", "", (value == null).ToString());
            else
            {
                nav.SetValue((value == null).ToString());
                nav.MoveToParent();
            }
            nav.SetTypedValue(PrepareToSave(value) ?? "");
        }

	    #region ISaver Members


		public PersistentStorage Ps
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		#endregion
	}
}

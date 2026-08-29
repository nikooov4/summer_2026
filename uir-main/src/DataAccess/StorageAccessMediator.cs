using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.DataSources.Sql;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public enum StorageAccessMediatorType
	{
		DataSet,
		MsSql,
        Xml
	}

	public abstract class StorageAccessMediator ///TODO: ICollection<StorageCommand>
	{
        //public static StorageAccessMediator Create(

		protected StorageAccessMediatorType _mediatorType;
		protected bool _locked = false;

		public bool Locked => _locked;

        public abstract void Reset();
		public abstract void BeginCollectData();
		public abstract void AddCommand(StorageCommand command);
		public abstract void EndCollectData();
		public abstract StorageCommandResult EndCollectAndFlushData();
		public abstract void CancelCollectData();
		public abstract StorageCommandResult FlushData();
	}


	public class MsSqlStorageAccessMediator : StorageAccessMediator
	{
		private readonly SqlConnection _connection = null;
		private readonly List<StorageCommand> _list = new List<StorageCommand>();
        private readonly Dictionary<EntityAttributeDescription, List<EntityBase>> _mtmRelationships = new Dictionary<EntityAttributeDescription, List<EntityBase>>();
		private string _script;
		private PersistentStorage _ps;
		private readonly Dictionary<EntityDescription, SqlScriptBuilder> _builders = new Dictionary<EntityDescription, SqlScriptBuilder>();

		public MsSqlStorageAccessMediator(PersistentStorage ps, SqlConnection connection)
		{
			_connection = connection;
			_mediatorType = StorageAccessMediatorType.MsSql;
			_ps = ps;
		}

		public PersistentStorage Ps
		{
			get { return _ps; }
		}

		public override void BeginCollectData()
		{
			if ( _locked )
				throw new Exception("Cannot begin collect data while in collecting state");
			_list.Clear();
			_mtmRelationships.Clear();
			_script = "";
			_locked = false;
		}

		public override void AddCommand(StorageCommand command)
		{
			if ( _locked )
				throw new Exception("The mediator is locked");
			if ( command.Mode == StorageCommandMode.Entity )
			{
				if ( !_builders.ContainsKey(command.EntityType) )
					_builders.Add(command.EntityType, new SqlScriptBuilder(new Mapper(command.EntityType, null), _ps));
				_list.Add(command);
			}
			else
			{
				if ( !_mtmRelationships.ContainsKey(command.ManyToManyProperty) )
				{
					var l = new List<EntityBase>();
					l.AddRange(command.Entities);
					_mtmRelationships.Add(command.ManyToManyProperty, l);
				}
				else
					foreach ( EntityBase e in command.Entities )
						if ( !_mtmRelationships[command.ManyToManyProperty].Contains(e) )
							_mtmRelationships[command.ManyToManyProperty].Add(e);
			}
		}

		public override void EndCollectData()
		{
			_locked = true;
			foreach ( StorageCommand command in _list )
			{
				string script;
				if ( command.CommandType == StorageCommandType.Insert )
					script = _builders[command.EntityType].CreateInsertStatement(command.Entities);
				else if ( command.CommandType == StorageCommandType.Update )
					script = _builders[command.EntityType].CreateUpdateStatement(command.Entities);
				else
					script = _builders[command.EntityType].CreateDeleteStatement(command.Entities);
				_script += script + "\r\n\r\n";
			}
			string clearMtm = "";
			string addMtm = "";
			foreach ( var pd in _mtmRelationships.Keys )
			{
				clearMtm += SqlScriptBuilder.CreateMtmDelete(pd, _ps, _mtmRelationships[pd]) + "\r\n";
				addMtm += SqlScriptBuilder.CreateMtmInsert(pd, _ps, _mtmRelationships[pd]) + "\r\n";
			}
			_script = clearMtm + "\r\n\r\n" + _script + "\r\n\r\n" + addMtm;
			string guid = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
			_script = "DECLARE @status int\r\n" +
				"BEGIN TRAN savetran" + guid + "\r\n\r\n" +
				"BEGIN TRY\r\n\r\n" +
				_script +
				"SELECT @status = 1\r\n" +
				"END TRY\r\n" +
				"BEGIN CATCH\r\n" +
				"SELECT @status = 0\r\n" +
				"SELECT /*ERROR_NUMBER() AS ErrorNumber ,ERROR_SEVERITY() AS ErrorSeverity ,ERROR_STATE() AS ErrorState ,ERROR_PROCEDURE() AS ErrorProcedure" +
					",ERROR_LINE() AS ErrorLine ,*/ERROR_MESSAGE() AS ErrorMessage\r\n" +
				"END CATCH\r\n" +
				"IF @status = 1 \r\n BEGIN\r\n COMMIT TRAN savetran" + guid + "\r\n SELECT 'COMMIT' \r\nEND ELSE \r\nBEGIN\r\nROLLBACK TRAN savetran" + guid + "\r\n SELECT 'ROLLBACK' \r\nEND\r\n";
		}

		public override StorageCommandResult EndCollectAndFlushData()
		{
			EndCollectData();
			return FlushData();
		}

		public override void CancelCollectData()
		{
			_locked = false;
			_script = "";
			_mtmRelationships.Clear();
			_list.Clear();
		}

		public override StorageCommandResult FlushData()
		{
		    try
		    {
		        using (var c = new SqlConnection(_connection.ConnectionString))
		        {
		            c.Open();
		            var command = new SqlCommand(_script, c);
		            var result = command.ExecuteScalar();
		            return new StorageCommandResult(StorageCommandResultState.Success, "Command succeded.", result);
		        }
		    }
		    catch (Exception ex)
		    {
		        return new StorageCommandResult(StorageCommandResultState.Failed, "Command failed.", ex);
		    }
		}

		public override void Reset()
		{

			_locked = false;
			BeginCollectData();
		}
	}

	public enum StorageCommandType
	{
		Insert,
		Update,
		Delete,
		Select,
		SelectUpdate
	}

	public enum StorageCommandResultState
	{
		Success = 1,
		Failed = 0
	}

	public class StorageCommandResult
	{
        internal StorageCommandResult(StorageCommandResultState state, string message, object result)
		{
			State = state;
			Message = message;
			Result = result;
		}

		public object Result { get; }

        public string Message { get; }

        public StorageCommandResultState State { get; }
    }

	public enum StorageCommandMode
	{
		Entity,
		MtmRelation
	}

	public class StorageCommand
	{
		//public static StorageCommand Create(object argument)

        private readonly EntityDescription _entityType;

		public StorageCommand(EntityBase argument)
		{
			if ( argument == null )
				throw new Exception("The entity should be null");  // Maybe "not null" ?
			Entities.Add(argument);
			_entityType = argument.EntityDescriptor;
			Mode = StorageCommandMode.Entity;
			SetCommandType();
		}

        internal StorageCommand(EntityBase entity, EntityAttributeDescription mtmProperty)
		{
			if ( entity == null )
				throw new Exception("The entity should not be null");
			ManyToManyProperty = mtmProperty;
			if ( !ManyToManyProperty.IsManyToManyRelation )
				throw new Exception("A many-to-many relation expected");
			Entities.Add(entity);
			_entityType = entity.EntityDescriptor;
			Mode = StorageCommandMode.MtmRelation;
			SetCommandType();
		}

		internal StorageCommand(List<EntityBase> argument)
		{
			if ( argument == null || argument.Count == 0 )
				throw new Exception("The entity list should not be empty");
			_entityType = argument[0].EntityDescriptor;
			foreach ( EntityBase e in argument )
				if ( e.EntityDescriptor != _entityType )
					throw new Exception("All element of the entity list must of the same type");
			Entities = argument;
			Mode = StorageCommandMode.Entity;
			SetCommandType();
		}

        internal StorageCommand(List<EntityBase> entities, EntityAttributeDescription mtmProperty)
		{
			if ( entities == null || entities.Count == 0 )
				throw new Exception("The entity list should not be empty");
			if ( !ManyToManyProperty.IsManyToManyRelation )
				throw new Exception("A many-to-many relation expected");
			_entityType = entities[0].EntityDescriptor;
			foreach ( EntityBase e in entities )
				if ( e.EntityDescriptor != _entityType )
					throw new Exception("All elements of the entity list must be of the same type");
			ManyToManyProperty = mtmProperty;
			Entities = entities;
			SetCommandType();
			Mode = StorageCommandMode.MtmRelation;
		}

		public EntityDescription EntityType
		{
			get { return _entityType; }
		}

		private void SetCommandType()
		{
			switch ( Entities[0].State )
			{
				case EntityState.New:
					CommandType = StorageCommandType.Insert;
					break;
				case EntityState.Deleted:
					CommandType = StorageCommandType.Delete;
					break;
				case EntityState.Changed:
					CommandType = StorageCommandType.Update;
					break;
				default:
					throw new Exception("Invalid entity state");
			}
		}

		public List<EntityBase> Entities { get; } = new List<EntityBase>();

        public StorageCommandType CommandType { get; protected set; }

        public EntityAttributeDescription ManyToManyProperty { get; }

        public StorageCommandMode Mode { get; }

        public string GenerateScript()
		{
			if ( Mode == StorageCommandMode.Entity )
				return null;
			return null;
		}
	}

    public class XmlStorageAccessMediator : StorageAccessMediator
    {
        private IDataAccessProvider _dataProvider;
        private List<StorageCommand> _list = new List<StorageCommand>();
        private Dictionary<EntityAttributeDescription, List<EntityBase>> _mtmRelationships = new Dictionary<EntityAttributeDescription, List<EntityBase>>();

        public XmlStorageAccessMediator(IDataAccessProvider dataProvider)
        {
            _mediatorType = StorageAccessMediatorType.Xml;
            _dataProvider = dataProvider;
        }

        private System.Xml.XmlWriterSettings GetDefaultWriterSettings()
        {
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.CloseOutput = false;
            settings.Indent = true;
            return settings;
        }

        public override void Reset()
        {
            _locked = false;
            BeginCollectData();
        }

        public override void BeginCollectData()
        {
            if (_locked)
                throw new Exception("Cannot begin collect data while in collecting state");
            _list.Clear();
            _mtmRelationships.Clear();
            _locked = false;
        }

        public override void AddCommand(StorageCommand command)
        {
            if (_locked)
                throw new Exception("The mediator is locked");
            if (command.Mode == StorageCommandMode.Entity)
            {
                _list.Add(command);
            }
            else
            {
                if (!_mtmRelationships.ContainsKey(command.ManyToManyProperty))
                {
                    var l = new List<EntityBase>();
                    l.AddRange(command.Entities);
                    _mtmRelationships.Add(command.ManyToManyProperty, l);
                }
                else
                    foreach (var e in command.Entities)
                        if (!_mtmRelationships[command.ManyToManyProperty].Contains(e))
                            _mtmRelationships[command.ManyToManyProperty].Add(e);
            }
        }

        public override void EndCollectData()
        {
            _locked = true;
        }

        public override StorageCommandResult EndCollectAndFlushData()
        {
            EndCollectData();
            return FlushData();
        }

        public override void CancelCollectData()
        {
            _locked = false;
            _mtmRelationships.Clear();
            _list.Clear();
        }

        public override StorageCommandResult FlushData()
        {
            try
            {
                var usedDataSources = new List<DataSource>();
                foreach (var command in _list)
                {
                    //ISaver saver = _ps.GetSaver(command.MappedEntity);
                    if (command.Mode == StorageCommandMode.Entity)
                        if (command.Entities.Count <= 1)
                        {
                            var ds = command.Entities[0].Source;
                            if (ds == null)
                                continue;
                            if (!usedDataSources.Contains(ds))
                                usedDataSources.Add(ds);
                            ds.GetSaver(command.EntityType).Save(command);
                        }
                        else
                            foreach (var entity in command.Entities)
                            {
                                if (!usedDataSources.Contains(entity.Source))
                                    usedDataSources.Add(entity.Source);
                                entity.Source.GetSaver(entity.Description.EntityDescriptor).Save(new StorageCommand(entity));
                            }
                    else
                    {
                        var ds = _dataProvider.GetDataSources(command.EntityType)[0];
                        var saver =  ds.GetSaver(command.EntityType);
                        if (!usedDataSources.Contains(ds))
                            usedDataSources.Add(ds);
                        saver.Save(command);
                    }
                }
                foreach ( var dataSource in usedDataSources )
                {
                    dataSource.Flush();
                }
                return new StorageCommandResult(StorageCommandResultState.Success, "Save succeded", null);
            }
            catch (Exception ex)
            {
                return new StorageCommandResult(StorageCommandResultState.Failed, "Save failed", ex);
            }
        }
    }
}

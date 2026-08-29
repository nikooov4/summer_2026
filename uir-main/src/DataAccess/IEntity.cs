using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.Core.UI.Attributes;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
    [Serializable]
    public enum EntityState
	{
        //Loading,
		Unchanged,
		Changed,
		New,
		Deleted,
		NewDeleted
	}

    //TODO: implement INotifyPropertyChanged
    public interface IEntity //: INotifyPropertyChanged
	{
		[IdField]
		[DisplayReadOnly]
		[FieldName("id")]
		object ID { get; set; }


        void Delete();


        [Internal]
        IEntityDescriptor Description { get; }

        event EntityBase.PropertyValueSetDelegate PropertyValueSet;
        event EntityBase.PropertyValueGetDelegate PropertyValueGet;
        event EntityBase.PropertyValueChangedDelegate PropertyValueChanged;
        event EventHandler<EntityIdChangedEventArgs> EntityIdChanged;

        [Internal]
        object this[string key] { get; set; }
        [Internal]
        object this[int propertyIndex] { get; set; }
        [Internal]
        object this[EntityAttributeDescription property] { get; set; }
	}

    public interface IEntityDescriptor
    {
        [Internal]
        EntityState State { get; }

        [Internal]
        EntityLoadingState LoadingState { get; }

        [Internal]
        Dictionary<string, object> SourceRawValues { get; set; }

        [Internal]
        DataSource Source { get; }

        event EntityBase.EntityStateChangedDelegate EntityStateChanged;

        void AcceptChanges();
        void RejectChanges();
        void RejectChanges(string propertyName);

        IDataContext DataContext { get; }

        [Internal]
        EntityDescription EntityDescriptor { get; set; }
    }

    public delegate TOuput Fun<in TInput, out TOuput>(TInput a);
    public delegate TOuput Fun<out TOuput>();
}

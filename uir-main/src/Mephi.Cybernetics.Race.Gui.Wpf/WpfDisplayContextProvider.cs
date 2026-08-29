using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.Core.UI;


namespace Mephi.Cybernetics.Race.Gui.Wpf
{
    public static class WpfDisplayContextProvider
    {
        private static DataDisplayControlMapping _readonly;
        private static DataDisplayControlMapping _editable;

        private static DataDisplayControlMapping CreateReadOnlyInstance()
        {
            var mapping = new DataDisplayControlMapping(DefaultWpfTypeSystem,
                new Dictionary<IDomain, Type>
                {
                    // 1. Как настроить сами контролы?
                    // 2. Как учесть readonly и выбирать разные контролы для редактируемых и нередактируемых объектов?
                    {ObjectDescription.Get<object>(), typeof(Label)},
                    {ObjectDescription.Get<byte>(), typeof(TextBlock)},
                    {ObjectDescription.Get<Int16>(), typeof(TextBlock)},
                    {ObjectDescription.Get<Int32>(), typeof(TextBlock)},
                    {ObjectDescription.Get<Int64>(), typeof(TextBlock)},
                    {ObjectDescription.Get<UInt16>(), typeof(TextBlock)},
                    {ObjectDescription.Get<UInt32>(), typeof(TextBlock)},
                    {ObjectDescription.Get<UInt64>(), typeof(TextBlock)},
                    {ObjectDescription.Get<Char>(), typeof(TextBlock)},
                    {ObjectDescription.Get<DateTime>(), typeof(TextBlock)},
                    {ObjectDescription.Get<float>(), typeof(TextBlock)},
                    {ObjectDescription.Get<double>(), typeof(TextBlock)},
                    {ObjectDescription.Get<bool>(), typeof(TextBlock)},
                    {ObjectDescription.Get<string>(), typeof(TextBlock)},
                    {ObjectDescription.Get<Vector3>(), typeof(VectorView)},
                    // Сработает ли ICollection<Object>?
                    {ObjectDescription.Get<ICollection<object>>(), typeof(DataGrid)},
                    //{ObjectDescription.Get<ICollection<byte>>(), typeof(DataGrid)},
                    //{ObjectDescription.Get<ICollection<float>>(), typeof(DataGrid)},
                    
                    //{ObjectDescription.Get<Delegate>(), typeof(Label)},
                    {ObjectDescription.Get<Enum>(), typeof(ComboBox)}
                    // {ObjectDescription.Get<Attribute>(), typeof(Label)},
                });
            return mapping;
        }
        
        private static DataDisplayControlMapping CreateEditableInstance()
        {
            var mapping = new DataDisplayControlMapping(DefaultWpfTypeSystem,
                new Dictionary<IDomain, Type>
                {
                    // 1. Как настроить сами контролы?
                    // 2. Как учесть readonly и выбирать разные контролы для редактируемых и нередактируемых объектов?
                    {ObjectDescription.Get<object>(), typeof(Label)},
                    {ObjectDescription.Get<byte>(), typeof(TextBox)},
                    {ObjectDescription.Get<Int16>(), typeof(TextBox)},
                    {ObjectDescription.Get<Int32>(), typeof(TextBox)},
                    {ObjectDescription.Get<Int64>(), typeof(TextBox)},
                    {ObjectDescription.Get<UInt16>(), typeof(TextBox)},
                    {ObjectDescription.Get<UInt32>(), typeof(TextBox)},
                    {ObjectDescription.Get<UInt64>(), typeof(TextBox)},
                    {ObjectDescription.Get<Char>(), typeof(TextBox)},
                    {ObjectDescription.Get<DateTime>(), typeof(TextBox)},
                    {ObjectDescription.Get<float>(), typeof(TextBox)},
                    {ObjectDescription.Get<double>(), typeof(TextBox)},
                    {ObjectDescription.Get<bool>(), typeof(TextBox)},
                    {ObjectDescription.Get<string>(), typeof(TextBox)},
                    {ObjectDescription.Get<Vector3>(), typeof(VectorView)},
                    // Сработает ли ICollection<Object>?
                    {ObjectDescription.Get<ICollection<object>>(), typeof(DataGrid)},
                    //{ObjectDescription.Get<ICollection<byte>>(), typeof(DataGrid)},
                    //{ObjectDescription.Get<ICollection<float>>(), typeof(DataGrid)},
                    
                    //{ObjectDescription.Get<Delegate>(), typeof(Label)},
                    {ObjectDescription.Get<Enum>(), typeof(ComboBox)}
                    // {ObjectDescription.Get<Attribute>(), typeof(Label)},
                });
            return mapping;
        }
        

        public static DataDisplayControlMapping ReadOnly
        {
            get
            {
                if (_readonly == null)
                    _readonly = CreateReadOnlyInstance();
                return _readonly;
            }
        }
        public static DataDisplayControlMapping Editable
        {
            get
            {
                if (_editable == null)
                    _editable = CreateEditableInstance();
                return _editable;
            }
        }

        public static DataDisplayControlMapping Simple (bool isReadOnly)
        {
            if (isReadOnly)
            {
                return ReadOnly;
            }
            
            return Editable;
        }
       public static IStrictTypeSystem DefaultWpfTypeSystem => ObjectDescription.TypeSystemDescription;
    }
}
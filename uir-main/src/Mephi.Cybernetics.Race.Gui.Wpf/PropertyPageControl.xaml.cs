using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.Core.UI;

namespace Mephi.Cybernetics.Race.Gui.Wpf
{
    /// <summary>
    /// Interaction logic for PropertyPageControl.xaml
    /// </summary>
    public partial class PropertyPageControl : UserControl
    {
        private static readonly DependencyProperty _typeProviderDependencyProperty;
        
        static PropertyPageControl()
        {
            _typeProviderDependencyProperty = DependencyProperty.Register(
                nameof(TypeProvider), 
                typeof(IStrictTypeSystem), 
                typeof(PropertyPageControl), 
                new PropertyMetadata(ObjectDescription.TypeSystemDescription),
                value =>
                {
                    if (value is IStrictTypeSystem typeSystem)
                        return typeSystem.IsAttributesSupported;
                    else
                        return false;
                });

        }
        private int _displayedPropertiesCount;
        private IStrictTypeSystem _typeProvider;
        private readonly DisplayControlResolver _controlResolver;

        public PropertyPageControl()
        {
            InitializeComponent();

            _controlResolver = new DisplayControlResolver();
            

            DependencyPropertyDescriptor
                .FromProperty(_typeProviderDependencyProperty, this.GetType())
                .AddValueChanged(this, (sender, args) => UpdateDisplayedProperties());

            DisplayedProperties = new ObservableCollection<PropertyDisplayInfo>();
            DisplayedProperties.CollectionChanged += (sender, args) => { UpdatePropertiesPresentation(args); };

            DataContextChanged += (sender, args) =>
            {
                if (args.NewValue != args.OldValue)
                    UpdateDisplayedProperties();
            };

            _typeProvider = ObjectDescription.TypeSystemDescription;
        }

        public ObservableCollection<PropertyDisplayInfo> DisplayedProperties { get; }

        public IStrictTypeSystem TypeProvider
        {
            get => _typeProvider;
            set
            {
                if (!value.IsAttributesSupported)
                    throw new ArgumentException("Attribute structure information required");
                var oldValue = _typeProvider;
                _typeProvider = value;
                OnPropertyChanged(new DependencyPropertyChangedEventArgs(_typeProviderDependencyProperty, oldValue, value));
            }
        }

        protected virtual void UpdateDisplayedProperties()
        {
            if (DataContext != null)
            {
                var typeInfo = _typeProvider.GetTypeInfo(DataContext);
                var propertyDisplayInfoList = _typeProvider.GetDomainAttributes(typeInfo)
                    .Select(PropertyDisplayInfo.FromDomainAttribute)
                    .OrderBy(p => p.PropertyIndex)
                    .ToList();
                DisplayedProperties.Clear();
                propertyDisplayInfoList.ForEach(p => DisplayedProperties.Add(p));
            }
            else
                DisplayedProperties.Clear();
        }

        protected virtual void UpdatePropertiesPresentation(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    ContainerGrid.Children.Clear();
                    ContainerGrid.RowDefinitions.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                    var counter = ContainerGrid.RowDefinitions.Count;
                    foreach (PropertyDisplayInfo displayedProperty in args.NewItems)
                    {
                        var uiInfo = _controlResolver.GetDisplayControlInfo(displayedProperty, DataContext);
                        if (uiInfo.CaptionControl is Control captionControl && uiInfo.ValueControl is Control valueControl)
                        {
                            var row = new RowDefinition()
                            {
                                Height = GridLength.Auto
                            };
                            ContainerGrid.RowDefinitions.Add(row);

                            if (ContainerGrid.RowDefinitions[counter] != row)
                                throw new NotImplementedException();
                            Grid.SetColumn(captionControl, 0);
                            Grid.SetRow(captionControl, counter);
                            ContainerGrid.Children.Add(captionControl);
                            Grid.SetColumn(valueControl, 10);
                            Grid.SetRow(valueControl, counter);
                            ContainerGrid.Children.Add(valueControl);

                        }
                        else
                            throw new ArgumentException("Invalid object as UI controls");
                        counter++;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }
    }
}

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.Core.UI.Attributes;

namespace Mephi.Cybernetics.Race.Gui.Wpf
{
    /// <summary>
    /// Interaction logic for TypeBasedContentPresenter.xaml
    /// </summary>
    [Presenter(typeof(object))]
    public partial class TypeBasedContentPresenter : UserControl
    {
        public static readonly DependencyProperty TypeProviderDependencyProperty =  
            DependencyProperty.Register(nameof(TypeProvider), typeof(IStrictTypeSystem), typeof(TypeBasedContentPresenter), new PropertyMetadata(ObjectDescription.TypeSystemDescription),
            value =>
            {
                if (value is IStrictTypeSystem typeSystem)
                    return typeSystem.IsAttributesSupported;
                else
                    return false;
            });
        public static readonly DependencyProperty ApplicationAreaProperty =
            DependencyProperty.Register(nameof(ApplicationArea), typeof(string), typeof(TypeBasedContentPresenter), 
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));

        public event DependencyPropertyChangedEventHandler ApplicationAreaChanged;

        private IStrictTypeSystem _typeProvider;

        public TypeBasedContentPresenter()
        {
            InitializeComponent();
            DependencyPropertyDescriptor.FromProperty(TypeProviderDependencyProperty, this.GetType()).AddValueChanged(this, (sender, args) => PresentContent());
            DataContextChanged += (sender, args) => PresentContent();
            ApplicationAreaChanged += (sender, args) => PresentContent();
        }


        public IStrictTypeSystem TypeProvider
        {
            get => _typeProvider;
            set
            {
                if (!value.IsAttributesSupported)
                    throw new ArgumentException("Attribute structure information required");
                var oldValue = _typeProvider;
                _typeProvider = value;
                OnPropertyChanged(new DependencyPropertyChangedEventArgs(TypeProviderDependencyProperty, oldValue, value));
            }
        }

        // public DisplayControlResolver Resolver { get; set; }

        public string ApplicationArea
        {
            get => GetValue(ApplicationAreaProperty) as string;
            set
            {
                var oldValue = GetValue(ApplicationAreaProperty) as string;
                if (oldValue != value)
                {
                    SetValue(ApplicationAreaProperty, value);
                    ApplicationAreaChanged?.Invoke(this, new DependencyPropertyChangedEventArgs(ApplicationAreaProperty, oldValue, value));
                }
            }
        }

        protected virtual Type ResolveContentPresenter(object viewModel)
        {
            //var viewModelDomain = _typeProvider.GetTypeInfo(viewModel);
            //var presenter = DefaultWpfDisplayContextProvider.Default.GetControlFor(viewModelDomain);
            var presenterAttributes = viewModel.GetType().GetCustomAttributes(typeof(PresenterAttribute), true).Cast<PresenterAttribute>().ToList();
            var areaSpecificPresenter = presenterAttributes.FirstOrDefault(a => a.ApplicationArea == ApplicationArea)?.ModelViewType;
            var presenter = areaSpecificPresenter ?? presenterAttributes.FirstOrDefault(a => string.IsNullOrEmpty(a.ApplicationArea))?.ModelViewType;
            if (presenter == null)
                presenter = typeof(PropertyPageControl);
            // if (presenter == null)
            //     Resolver.GetValueControl()
            if (!presenter.IsSubclassOf(typeof(Control)))
                throw new ArgumentException("Prensenter must decsend from Control");
            return presenter;
        }

        private void PresentContent()
        {
            if (DataContext != null)
            {
                var type = ResolveContentPresenter(DataContext);
                if (type != null)
                {
                    //var control = type.GetConstructor(new Type[] { }).Invoke(new object[] { }) as Control;
                    if (Activator.CreateInstance(type) is Control control)
                    {
                        control.DataContext = DataContext;
                        Content = control;
                    }
                    /*var control = Activator.CreateInstance<TextBlock>();
                            if (control is Control)
                            {
                                control.DataContext = DataContext;
                                Content = control;
                            } 
                            if (control is TextBlock)
                            {
                                control.Text = DataContext.ToString() as string;
                            }*/                   
                    
                }
            }
            else
                Content = null;
        }

    }
}

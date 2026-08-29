using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.UI;

namespace Mephi.Cybernetics.Race.Gui.Wpf
{
    public class DisplayControlResolver
    {
        public virtual object GetCaptionControl(PropertyDisplayInfo propertyDisplayInfo, object obj)
        {
            return new Label()
            {
                Content = string.IsNullOrWhiteSpace(propertyDisplayInfo.DisplayName)
                    ? propertyDisplayInfo.Name
                    : propertyDisplayInfo.DisplayName,
                ToolTip = propertyDisplayInfo.Description
            };
        }

        private static Style TextStyleDefault
        {
            get
            {
                var textStyle = new Style();
                textStyle.Setters.Add(new Setter { Property = FrameworkElement.WidthProperty, Value = 400 });
                return textStyle;
            }
        }

        public virtual object GetValueControl(PropertyDisplayInfo propertyDisplayInfo, object obj)
        {
            var dataDisplayControlMapping = WpfDisplayContextProvider.Simple(propertyDisplayInfo.IsReadOnly);
            try
            {
                var value = propertyDisplayInfo.GetPropertyValue(obj);
                var type = ObjectDescription.Get(value.GetType());
                var control = dataDisplayControlMapping.GetControlFor(type);
                if (control != null)
                {
                    switch (control)
                    {
                        case TextBlock block:
                            block.Text = value.ToString();
                         //  block.Style = TextStyleDefault;
                            break;
                        case TextBox box:
                            box.Text = value.ToString();
                            box.IsReadOnly = propertyDisplayInfo.IsReadOnly;
                            box.Width = 300;
                            //box.HorizontalAlignment;
                            //box.Style = TextStyleDefault;
                            break;
                        case Control ctr:
                            if (value is Vector3)
                            {
                                ctr.DataContext = new VectorWrapper((Vector3)value); 
                            }
                            break;
             
                    }
                    

                    return control;
                }
            }
            catch (Exception)
            {
                
                return new Label()
                {
                    Content = propertyDisplayInfo.GetPropertyValue(obj).ToString()
                };
            }

            return new Label()
            {
                Content = propertyDisplayInfo.GetPropertyValue(obj).ToString()
            };
            // return new TypeBasedContentPresenter();
            /*var presenter = new TypeBasedContentPresenter()
            {
                DataContext = obj
            };
            return presenter;*/
        }

        // public DisplayControlResolver For<T>(Control ctrl)
        // {
        // }
        //
        public UiElementInfo GetDisplayControlInfo(PropertyDisplayInfo propertyDisplayInfo, object obj)
        {
            var value = propertyDisplayInfo.GetPropertyValue(obj);
            var captionControl = GetCaptionControl(propertyDisplayInfo, obj);
            var valueControl = GetValueControl(propertyDisplayInfo, obj); // ошибка
            //var valueControl =  GetCaptionControl(propertyDisplayInfo, value);

            return new UiElementInfo(propertyDisplayInfo, captionControl, valueControl);
        }
    }
}
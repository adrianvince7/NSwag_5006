using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;


namespace NSwagStudio.Converters
{
    public class IsValueToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Target { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value == null && Target == null) || (value.Equals(Target)))
                return true;
            
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
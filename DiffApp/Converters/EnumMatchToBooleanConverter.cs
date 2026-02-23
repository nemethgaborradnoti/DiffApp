using System;
using System.Globalization;
using System.Windows.Data;

namespace DiffApp.Converters
{
    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public static EnumMatchToBooleanConverter Instance { get; } = new EnumMatchToBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // Handle Type checks for ViewModel navigation
            if (parameter is Type typeParameter && value != null)
            {
                return value.GetType() == typeParameter;
            }

            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
                return parameter;

            return Binding.DoNothing;
        }
    }
}
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Reolmarked.UI.Views
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString()?.ToLower();

            return status switch
            {
                "grøn" or "green" or "ledig" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")),
                "rød" or "red" or "optaget" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B0000")),
                "gul" or "yellow" or "reserveret" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700")),
                _ => Brushes.LightGray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
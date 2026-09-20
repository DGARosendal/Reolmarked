using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Reolmarked.Core.Models;

namespace Reolmarked.UI.Views
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Direct enum match if passed as Status enum
            if (value is Status statusEnum)
            {
                return statusEnum switch
                {
                    Status.Ledig => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")),      // Green
                    Status.Booket => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B0000")),     // Red
                    Status.Opsagt => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700")),     // Yellow
                    Status.UdeAfDrift => Brushes.DarkGray,                                                        // Gray
                    _ => Brushes.LightGray
                };
            }

            // String fallback for safety
            string statusStr = value?.ToString()?.ToLower();

            return statusStr switch
            {
                "ledig" or "grøn" or "green" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")),
                "booket" or "optaget" or "rød" or "red" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B0000")),
                "opsagt" or "reserveret" or "gul" or "yellow" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700")),
                "udeafdrift" or "grå" or "gray" => Brushes.DarkGray,
                _ => Brushes.LightGray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
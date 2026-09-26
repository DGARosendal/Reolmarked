// SRP: This converter is responsible for turning a bool into a Visibility.
// Used by the confirmation popup to show/hide based on IsConfirmationVisible.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reolmarked.UI.Views
{
    // Converts a bool to WPF Visibility: true becomes Visible, false becomes Collapsed.
    public class BoolToVisibilityConverter : IValueConverter
    {
        // Convert runs when data flows FROM the ViewModel TO the UI.
        // WPF calls this automatically whenever the bound value changes.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // First, check if the value really is a bool. If it isn't,
            // we can't convert it, so we hide the element.
            if (!(value is bool))
            {
                return Visibility.Collapsed;
            }

            // Now that we know it's a bool, cast it to a local variable.
            bool flag = (bool)value;

            // true means "show the element", false means "hide it".
            if (flag)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        // ConvertBack would run when data flows from the UI back to the ViewModel.
        // We don't use it for this converter, but the IValueConverter interface
        // requires that the method exists. We return a sensible default.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the UI says the element is Visible, the original bool was true.
            if (value is Visibility v && v == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
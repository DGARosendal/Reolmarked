using System;
using System.Windows;
using System.Windows.Controls;

namespace Reolmarked.UI.Views
{
    public partial class RentalView : UserControl
    {
        public RentalView()
        {
            InitializeComponent();
        }

        private void DatePicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                ApplyBlackoutDates(datePicker);
            }
        }

        private void DatePicker_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                ApplyBlackoutDates(datePicker);
            }
        }

        private void ApplyBlackoutDates(DatePicker datePicker)
        {
            try
            {
                datePicker.BlackoutDates.Clear();

                DateTime today = DateTime.Today;

                // 1. Black out past months and current month completely
                DateTime endOfCurrentMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                datePicker.BlackoutDates.Add(new CalendarDateRange(DateTime.MinValue, endOfCurrentMonth));

                // 2. Loop 36 months ahead and black out days 2 through month-end for every month
                DateTime startMonth = new DateTime(today.Year, today.Month, 1).AddMonths(1);
                for (int i = 0; i < 36; i++)
                {
                    DateTime monthToBlackout = startMonth.AddMonths(i);
                    int daysInMonth = DateTime.DaysInMonth(monthToBlackout.Year, monthToBlackout.Month);

                    if (daysInMonth >= 2)
                    {
                        DateTime rangeStart = new DateTime(monthToBlackout.Year, monthToBlackout.Month, 2);
                        DateTime rangeEnd = new DateTime(monthToBlackout.Year, monthToBlackout.Month, daysInMonth);

                        datePicker.BlackoutDates.Add(new CalendarDateRange(rangeStart, rangeEnd));
                    }
                }
            }
            catch (Exception)
            {
                // Prevents crashes if WPF's internal selection temporarily collides with blackout ranges
            }
        }
    }
}
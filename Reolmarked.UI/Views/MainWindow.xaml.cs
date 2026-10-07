using Microsoft.Extensions.Configuration;
using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.ViewModels;
using System.Windows;

namespace Reolmarked.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string? ConnectionString = config.GetConnectionString("DefaultConnection");
            if (ConnectionString is null)
            {
                MessageBox.Show("Kan ikke læse AppSettings");
            }
            else
            {
                // Create one repository per table and pass them into MainViewModel.
                IShelfRepository shelfRepository = new ShelfRepository(ConnectionString);
                IShelfRenterRepository shelfRenterRepository = new ShelfRenterRepository(ConnectionString);
                IRentalRepository rentalRepository = new RentalRepository(ConnectionString);

                DataContext = new MainViewModel(shelfRepository, shelfRenterRepository, rentalRepository);
            }

            
        }
    }
}
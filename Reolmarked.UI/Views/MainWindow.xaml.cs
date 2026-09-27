using System.Windows;
using Reolmarked.UI.ViewModels;
using Reolmarked.Core.Repositories;

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

            // Create one repository per table and pass them into MainViewModel.
            IShelfRepository shelfRepository = new ShelfRepository();
            IShelfRenterRepository shelfRenterRepository = new ShelfRenterRepository();
            IRentalRepository rentalRepository = new RentalRepository();

            DataContext = new MainViewModel(shelfRepository, shelfRenterRepository, rentalRepository);
        }
    }
}
// SRP: MainViewModel is responsible for navigation between the three pages.
// It holds one ViewModel per page and swaps CurrentView when a button is clicked.

using Reolmarked.Core.Repositories;
using Reolmarked.UI.Commands;

namespace Reolmarked.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Commands for the top navigation bar.
        public RelayCommand ShelfViewCommand { get; set; }
        public RelayCommand ShelfRenterViewCommand { get; set; }
        public RelayCommand RentalViewCommand { get; set; }
        public RelayCommand ManageRentalViewCommand { get; set; }

        // One ViewModel per page.
        public ShelfViewModel ShelfVM { get; set; }
        public ShelfRenterViewModel ShelfRenterVM { get; set; }
        public RentalViewModel RentalVM { get; set; }
        public ManageRentalViewModel ManageRentalVM { get; set; }


        // The ViewModel currently shown in the ContentControl.
        private object? _currentView;
        public object? CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainViewModel(
            IShelfRepository shelfRepository,
            IShelfRenterRepository shelfRenterRepository,
            IRentalRepository rentalRepository)
        {
            // Create one ViewModel per page, passing the repositories they need.
            ShelfVM = new ShelfViewModel(shelfRepository);
            ShelfRenterVM = new ShelfRenterViewModel(shelfRenterRepository);
            RentalVM = new RentalViewModel(rentalRepository, shelfRepository, shelfRenterRepository);
            ManageRentalVM = new ManageRentalViewModel(rentalRepository, shelfRepository, shelfRenterRepository);

            // Start on the Book reol page (matches the wireframe).
            CurrentView = RentalVM;

            // Each command refreshes the target ViewModel's data before showing it.
            // This ensures changes made on one page (e.g. booking a shelf) are
            // visible on another page that shows the same data.

            ShelfViewCommand = new RelayCommand(o =>
            {
                ShelfVM.LoadShelves();
                CurrentView = ShelfVM;
            });

            ShelfRenterViewCommand = new RelayCommand(o =>
            {
                ShelfRenterVM.LoadRenters();
                CurrentView = ShelfRenterVM;
            });

            RentalViewCommand = new RelayCommand(o =>
            {
                RentalVM.LoadShelves();
                CurrentView = RentalVM;
            });

            ManageRentalViewCommand = new RelayCommand(o =>
            {
                ManageRentalVM.LoadRentals();
                CurrentView = ManageRentalVM;
            });
        }
    }
}
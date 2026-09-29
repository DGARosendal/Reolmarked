// SRP: This ViewModel is responsible for the "Book reol" screen (UC-3 Basic flow + 1A + 4A).

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.Commands;

namespace Reolmarked.UI.ViewModels
{
    // ViewModel for the "Book reol" screen (UC-3).
    public class RentalViewModel : ViewModelBase
    {
        // The repository we use to save a new rental (the C in CRUD).
        private readonly IRentalRepository _rentalRepository;

        // We also need these to look up shelves and renters.
        private readonly IShelfRepository _shelfRepository;
        private readonly IShelfRenterRepository _renterRepository;

        // Shelves

        // ObservableCollection auto-updates the UI when items are added/removed.
        public ObservableCollection<Shelf> Shelves { get; }

        // Currently selected shelf in the grid. null means "nothing selected".
        private Shelf? _selectedShelf;
        public Shelf? SelectedShelf
        {
            get => _selectedShelf;
            set
            {
                // SetField updates the field and notifies WPF about the change.
                if (SetField(ref _selectedShelf, value))
                {
                    // Re-check if Book button should be enabled now.
                    BookCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        // Renter search

        // Live search: runs on every keystroke from the TextBox binding.
        // Starts as an empty string so we never have to null-check it.
        private string _searchPhoneNumber = string.Empty;
        public string SearchPhoneNumber
        {
            get => _searchPhoneNumber;
            set
            {
                // Only re-search if the value actually changed.
                if (SetField(ref _searchPhoneNumber, value))
                {
                    // SearchRenter takes an object parameter because it was originally
                    // designed to be called from a RelayCommand, which always passes
                    // a value along. That value typically represents the UI element that
                    // triggered the command (a button, a menu item, and so on).
                    //
                    // Here, however, we are calling SearchRenter manually from the setter
                    // to get live search working. Since the method does not actually use
                    // the parameter, we pass null, i.e. the value is simply ignored.
                    SearchRenter(null);
                }
            }
        }

        // Holds the renters that match the current search.
        // ObservableCollection means the DataGrid updates automatically
        // whenever items are added or removed from this list.
        public ObservableCollection<ShelfRenter> SearchResults { get; }

        // Chosen renter from the search results (or newly created).
        private ShelfRenter? _selectedRenter;
        public ShelfRenter? SelectedRenter
        {
            get => _selectedRenter;
            set
            {
                if (SetField(ref _selectedRenter, value))
                {
                    // Same as for SelectedShelf: Book button may change state.
                    BookCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        // Create renter fields

        // Fields for creating a new renter if they're not in the system.
        // Start as empty strings so we never have to null-check them.
        private string _newFirstName = string.Empty;
        public string NewFirstName
        {
            get => _newFirstName;
            set => SetField(ref _newFirstName, value);
        }

        private string _newLastName = string.Empty;
        public string NewLastName
        {
            get => _newLastName;
            set => SetField(ref _newLastName, value);
        }

        private string _newPhoneNumber = string.Empty;
        public string NewPhoneNumber
        {
            get => _newPhoneNumber;
            set => SetField(ref _newPhoneNumber, value);
        }

        // Booking

        // Start date for the new rental. Defaults to today.
        // DateTime.Today gives midnight of the current day.
        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate
        {
            get => _startDate;
            set => SetField(ref _startDate, value);
        }

        // Controls the confirmation popup's visibility.
        // XAML converts this bool to Visible/Collapsed via BoolToVisibilityConverter.
        private bool _isConfirmationVisible;
        public bool IsConfirmationVisible
        {
            get => _isConfirmationVisible;
            set => SetField(ref _isConfirmationVisible, value);
        }

        // Commands

        // Commands are called from XAML buttons, e.g. Command="{Binding BookCommand}".
        public RelayCommand CreateRenterCommand { get; }
        public RelayCommand SelectShelfCommand { get; }
        public RelayCommand BookCommand { get; }
        public RelayCommand ConfirmBookingCommand { get; }
        public RelayCommand CancelBookingCommand { get; }

        // Constructor
        public RentalViewModel(
            IRentalRepository rentalRepository,
            IShelfRepository shelfRepository,
            IShelfRenterRepository renterRepository)
        {
            // Store the repositories so we can use them in the methods below.
            _rentalRepository = rentalRepository;
            _shelfRepository = shelfRepository;
            _renterRepository = renterRepository;

            // Initialize empty collections so XAML bindings don't get null.
            Shelves = new ObservableCollection<Shelf>();
            SearchResults = new ObservableCollection<ShelfRenter>();

            // Wire each command to its method.
            // The optional second argument (CanExecute) decides when the button is enabled.
            // Only BookCommand uses one, the others are always clickable.
            CreateRenterCommand = new RelayCommand(CreateRenter);
            SelectShelfCommand = new RelayCommand(SelectShelf);
            BookCommand = new RelayCommand(ShowConfirmation, CanBook);
            ConfirmBookingCommand = new RelayCommand(ConfirmBooking);
            CancelBookingCommand = new RelayCommand(CancelBooking);

            // Fill the shelf grid right away, so the screen isn't empty on load.
            LoadShelves();
        }

        // Loading

        // Fetches every shelf from the database.
        // Made public so MainViewModel can refresh the list when navigating here.
        public void LoadShelves()
        {
            Shelves.Clear();
            var shelvesFromDb = _shelfRepository.GetAll();
            foreach (var shelf in shelvesFromDb)
            {
                Shelves.Add(shelf);
            }
        }

        // Search / create renter

        // Filters renters by phone number. Called on every keystroke (live search).
        // Empty input shows everyone, otherwise we match on a substring of the number.
        private void SearchRenter(object? parameter)
        {
            try
            {
                // Clear old results so we only show the ones from this search.
                SearchResults.Clear();

                var allRenters = _renterRepository.GetAll();

                // Prepare the search text once. Trim removes stray spaces,
                // and ToLower makes the search ignore upper/lowercase.
                string search = SearchPhoneNumber.Trim().ToLower();

                // Go through every renter from the database.
                foreach (var renter in allRenters)
                {
                    // If the search is empty, everyone matches.
                    // Otherwise, only renters whose phone number contains the search text.
                    if (search == "" || renter.PhoneNumber.ToLower().Contains(search))
                    {
                        SearchResults.Add(renter);
                    }
                }
            }
            catch (Exception ex)
            {
                // Show a friendly error instead of crashing the whole app.
                MessageBox.Show($"Kunne ikke søge efter reollejer: {ex.Message}",
                    "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Creates a new renter and saves them, then pre-selects them.
        private void CreateRenter(object? parameter)
        {
            try
            {
                // The ShelfRenter constructor validates name and phone number.
                var newRenter = new ShelfRenter(NewFirstName, NewLastName, NewPhoneNumber);

                // Add to DB — the repository fills in the new RenterId (generated
                // by auto-incrementing IDENTITY in the database).
                _renterRepository.Add(newRenter);

                // Show only the new renter and select them automatically,
                // so the user can go straight to booking.
                SearchResults.Clear();
                SearchResults.Add(newRenter);
                SelectedRenter = newRenter;

                // Clear the input fields, ready for the next renter.
                NewFirstName = string.Empty;
                NewLastName = string.Empty;
                NewPhoneNumber = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke oprette reollejer: {ex.Message}",
                    "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Shelf selection

        // Called when a shelf button is clicked in the grid. The view sends the
        // clicked Shelf object as the command parameter, so we just need to store
        // it in SelectedShelf. From there, WPF can re-evaluate CanBook and enable
        // the Book button if both a shelf and a renter are selected.
        private void SelectShelf(object? parameter)
        {
            // "is Shelf shelf" checks that the parameter really is a Shelf object.
            // If so, it is cast to a Shelf and stored in the local variable "shelf".
            // This is safer than a direct cast, which would throw an exception if
            // the parameter happened to be something else.
            if (parameter is Shelf shelf)
            {
                // Only available shelves can be booked. UdeAfDrift and Opsagt are not available.
                if (shelf.Status != Status.Ledig)
                {
                    MessageBox.Show(
                        $"Reol {shelf.ShelfNumber} kan ikke bookes, fordi den har status '{shelf.Status}'. " +
                        "Kun ledige reoler kan bookes.",
                        "Reol kan ikke bookes",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                SelectedShelf = shelf;
            }
        }

        // Booking

        // Book button is only active when both a shelf and a renter are picked.
        // WPF checks this automatically whenever RaiseCanExecuteChanged fires.
        // SelectedShelf.Status == Status.Ledig makes sure that we only can book shelves that
        // are available (so out of commission-shelves, soon to be available-shelves and
        // already booked-shelves ones are not book-able).
        private bool CanBook(object? parameter)
        {
            return SelectedShelf != null && SelectedRenter != null && SelectedShelf.Status == Status.Ledig; ;
        }

        // Opens the confirmation popup. Nothing is saved until Bekræft is clicked.
        private void ShowConfirmation(object? parameter)
        {
            IsConfirmationVisible = true;
        }

        // Called when Bekræft is clicked. This is where the rental is created.
        private void ConfirmBooking(object? parameter)
        {
            try
            {
                // Safety check: we need both a shelf and a renter to create a rental.
                // The Book button is already disabled until both are chosen (via CanBook),
                // but we guard here too, in case ConfirmBooking is ever called another way.
                if (SelectedShelf == null || SelectedRenter == null) return;

                // In case ConfirmBooking should ever be called from a context other than the
                // Book button, then we also catch an attempt to book an unavailable shelf here.
                if (SelectedShelf.Status != Status.Ledig)
                {
                    MessageBox.Show(
                        $"Reol {SelectedShelf.ShelfNumber} kan ikke bookes, fordi den har status '{SelectedShelf.Status}'.",
                        "Reol kan ikke bookes",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // 1. Build the Rental object.
                var rental = new Rental(
                    SelectedShelf.ShelfNumber,
                    StartDate,
                    SelectedRenter.RenterId);

                // 2. Insert the new rental into the RENTAL table.
                _rentalRepository.Add(rental);

                // 3. Mark the shelf as booked so it turns red in the grid.
                SelectedShelf.Status = Status.Booket;
                _shelfRepository.Update(SelectedShelf);

                // 4. Force a refresh so the color change shows immediately.
                //
                // Normally, WPF updates the UI automatically when a property on a bound
                // object changes. But when the object sits inside an ObservableCollection,
                // WPF does not always notice in-place changes to the object's properties
                // (like Shelf.Status going from Ledig to Booket).
                //
                // To force a refresh, we replace the item at its own index with itself:
                // first set it to null, then set it back. That makes the ObservableCollection
                // register a change, which triggers WPF to re-render the shelf  and the
                // color converter runs again with the new Status.
                int index = Shelves.IndexOf(SelectedShelf);
                if (index >= 0)
                {
                    Shelves[index] = null;
                    Shelves[index] = SelectedShelf;
                }

                // 5. Close the confirmation popup and let the user know the booking
                // was successful. IsConfirmationVisible is bound to the popup's
                // Visibility in the view, so setting it to false hides it immediately.
                //
                // The MessageBox is a simple confirmation dialog with the shelf number
                // and the renter's full name. It uses the Information icon to signal
                // that this is a success message, not a warning or error.
                IsConfirmationVisible = false;
                MessageBox.Show(
                    $"Reol {rental.ShelfNumber} er nu udlejet til {SelectedRenter.FirstName} {SelectedRenter.LastName}.",
                    "Udlejning oprettet",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 6. Clear the shelf selection so a new booking can start.
                SelectedShelf = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke oprette udlejning: {ex.Message}",
                    "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Called when Annuller is clicked in the popup. Nothing has been saved.
        private void CancelBooking(object? parameter)
        {
            IsConfirmationVisible = false;
        }
    }
}
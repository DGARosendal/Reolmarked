using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Reolmarked.UI.ViewModels
{
    public class ManageRentalViewModel : ViewModelBase
    {
        // Repositories
        private readonly IRentalRepository _rentalRepository;
        private readonly IShelfRepository _shelfRepository;
        private readonly IShelfRenterRepository _renterRepository;

        // Collection of Rentals
        public ObservableCollection<Rental> Rentals { get; set; }

        // Selected Rental
        private Rental? _selectedRental;
        public Rental? SelectedRental
        {
            get => _selectedRental;
            set
            {
                // SetField updates the field and notifies WPF about the change.
                if (SetField(ref _selectedRental, value))
                {
                    if (_selectedRental != null)
                    {
                        // Get information about Shelf and ShelfRenter from repositories
                        ShelfRenter selectedRenter = _renterRepository.GetById(_selectedRental.RenterId);
                        Shelf selectedShelf = _shelfRepository.GetById(_selectedRental.ShelfNumber);

                        // Update Properties
                        StartDate = _selectedRental.StartDate.ToString("d");
                        FirstName = selectedRenter.FirstName;
                        LastName = selectedRenter.LastName;
                        PhoneNumber = selectedRenter.PhoneNumber;
                        ShelfNumber = _selectedRental.ShelfNumber;
                        Configuration = selectedShelf.Configuration;
                        Status = selectedShelf.Status;                        
                    }
                    UpdateCommand?.RaiseCanExecuteChanged();
                    TerminateCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        // Fields

        
        private string _startDate = string.Empty;
        public string StartDate
        {
            get => _startDate;
            set => SetField(ref _startDate, value);
        }
        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetField(ref _firstName, value);
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetField(ref _lastName, value);
        }

        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetField(ref _phoneNumber, value);
        }

        private int? _shelfNumber;
        public int? ShelfNumber
        {
            get => _shelfNumber;
            set => SetField(ref _shelfNumber, value);
        }
        private Status? _status;
        public Status? Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        private Configuration? _configuration;
        public Configuration? Configuration
        {
            get => _configuration;
            set => SetField(ref _configuration, value);
        }


        // RelayCommands

        public RelayCommand UpdateCommand { get; set; }
        public RelayCommand TerminateCommand { get; set; }
        public RelayCommand ClearSelectionCommand { get; set; }


        public ManageRentalViewModel(
            IRentalRepository rentalRepository,
            IShelfRepository shelfRepository,
            IShelfRenterRepository renterRepository)
        {
            // Store the repositories so we can use them in the methods below.
            _rentalRepository = rentalRepository;
            _shelfRepository = shelfRepository;
            _renterRepository = renterRepository;

            // Commands
            UpdateCommand = new RelayCommand(Update, CanUpdate);
            TerminateCommand = new RelayCommand(Terminate, CanTerminate);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            // Initialize 
            Rentals = new ObservableCollection<Rental>();

            LoadRentals();
        }

        // Execute commands

        private void ClearSelection()
        {
            StartDate = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            ShelfNumber = null;
            Configuration = null;
            Status = null;
            SelectedRental = null;
        }

        private void Terminate()
        {
            if (SelectedRental == null)
            {
                throw new NotImplementedException();
            }

            DateTime dt = DateTime.Today;
            DateTime terminateDate;
            
            if (DateTime.Today.Day < 20)
            {
                terminateDate = new DateTime(dt.Year,dt.Month,1).AddMonths(1);
            }
            else
            {
                terminateDate = new DateTime(dt.Year, dt.Month, 1).AddMonths(2);
            }
            string terminateMessage = $"Opsig reol {SelectedRental.ShelfNumber} til d. {terminateDate.ToString("d")}?";
            MessageBoxResult messageResult = MessageBox.Show(terminateMessage, "Opsig reol", MessageBoxButton.YesNo);
            if (messageResult == MessageBoxResult.Yes)
            {
                // Note:Does not delete the record, ONLY sets Shelf to "Opsagt"
                Shelf oldShelf = _shelfRepository.GetById(SelectedRental.ShelfNumber);
                _shelfRepository.Update(new Shelf(oldShelf.ShelfNumber, oldShelf.Configuration, Core.Models.Status.Opsagt));
            }
        }

        private void Update()
        {
            if (ShelfNumber == null || SelectedRental == null)
            {
                throw new NotImplementedException();
            }
            // Update old shelf in repository
            Shelf oldShelf = _shelfRepository.GetById(SelectedRental.ShelfNumber);
            _shelfRepository.Update(new Shelf(oldShelf.ShelfNumber, oldShelf.Configuration, Core.Models.Status.Ledig));
            // Update new shelf in repository
            _shelfRepository.Update(new Shelf((int)ShelfNumber, _shelfRepository.GetById((int)ShelfNumber).Configuration, Core.Models.Status.Booket));

            // Update rental - Delete and create new because ShelfNumber is PK
            Rental newRental = new Rental((int)ShelfNumber, DateTime.Today, SelectedRental.RenterId, SelectedRental.UserId);
            _rentalRepository.Delete(SelectedRental.ShelfNumber);
            _rentalRepository.Add(newRental);
        }

        // CanExecute commands

        private bool CanUpdate()
        {
            if (SelectedRental == null || ShelfNumber == null)
            {
                return false;
            }
            return true;
        }

        private bool CanTerminate()
        {
            if (SelectedRental == null)
            {
                return false;
            }
            return true;
        }

        // Fetches every shelf from the database.
        // Made public so MainViewModel can refresh the list when navigating here.
        public void LoadRentals()
        {
            Rentals.Clear();
            var rentalsFromDb = _rentalRepository.GetAll();
            foreach (var rental in rentalsFromDb)
            {
                Rentals.Add(rental);
            }
        }
    }
}

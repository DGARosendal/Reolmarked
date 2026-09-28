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
        // -----  Repositories  -----
        private readonly IRentalRepository _rentalRepository;
        private readonly IShelfRepository _shelfRepository;
        private readonly IShelfRenterRepository _renterRepository;

        // -----  For DataGrid  -----

        // Collection of Rentals
        public record RentalRecord(int RenterId, int UserId, DateTime StartDate, string FirstName, string LastName, string PhoneNumber, int ShelfNumber, Configuration Configuration, Status Status);
        public ObservableCollection<RentalRecord> RentalRecords { get; set; }

        // Selected Rental
        private RentalRecord? _selectedRentalRecord;
        public RentalRecord? SelectedRentalRecord
        {
            get => _selectedRentalRecord;
            set
            {
                // SetField updates the field and notifies WPF about the change.
                if (SetField(ref _selectedRentalRecord, value))
                {
                    if (_selectedRentalRecord != null)
                    {
                        // Update Properties
                        StartDate = _selectedRentalRecord.StartDate.ToString("d");
                        FirstName = _selectedRentalRecord.FirstName;
                        LastName = _selectedRentalRecord.LastName;
                        PhoneNumber = _selectedRentalRecord.PhoneNumber;
                        ShelfNumber = _selectedRentalRecord.ShelfNumber;
                        Configuration = _selectedRentalRecord.Configuration;
                        Status = _selectedRentalRecord.Status;                        
                    }
                    UpdateCommand?.RaiseCanExecuteChanged();
                    TerminateCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        // -----  Properties for textboxes  -----

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


        // -----  RelayCommands  -----

        public RelayCommand UpdateCommand { get; set; }
        public RelayCommand TerminateCommand { get; set; }
        public RelayCommand ClearSelectionCommand { get; set; }


        // -----  ctor with repositories  -----
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
            RentalRecords = new ObservableCollection<RentalRecord>();

            LoadRentalRecords();
        }

        // -----  Execute commands  -----

        // Clear all textboxes
        private void ClearSelection()
        {
            StartDate = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            ShelfNumber = null;
            Configuration = null;
            Status = null;
            SelectedRentalRecord = null;
        }

        // Terminate rental
        // OBS: Does not delete the record, ONLY sets Shelf to "Opsagt"
        private void Terminate()
        {
            if (SelectedRentalRecord == null)
            {
                throw new Exception("CanTerminate() not triggered.");
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
            string terminateMessage = $"Opsig reol {SelectedRentalRecord.ShelfNumber} til d. {terminateDate.ToString("d")}?";
            MessageBoxResult messageResult = MessageBox.Show(terminateMessage, "Opsig reol", MessageBoxButton.YesNo);
            if (messageResult == MessageBoxResult.Yes)
            {
                // OBS: Does not delete the record, ONLY sets Shelf to "Opsagt"
                // Create new record from SelectedRentalRecord with updated properties
                RentalRecord updatedRecord = SelectedRentalRecord with { Status = Core.Models.Status.Opsagt };
                // Update shelf in repository
                _shelfRepository.Update(new Shelf(updatedRecord.ShelfNumber, updatedRecord.Configuration, updatedRecord.Status));
                // Remove old and add new RentalRecord from RentalRecords
                RentalRecords.Remove(SelectedRentalRecord);
                RentalRecords.Add(updatedRecord);
            }
        }

        // Update rental = Delete and create new, because ShelfNumber is PK
        private void Update()
        {
            if (ShelfNumber == null || SelectedRentalRecord == null)
            {
                throw new Exception("CanUpdate() not triggered.");
            }

            // Check if new shelf for booking is "Ledig"
            if (_shelfRepository.GetById((int)ShelfNumber).Status != Core.Models.Status.Ledig)
            {
                MessageBox.Show($"Reol nr. {(int)ShelfNumber} er ikke ledig. Vælg en anden.", "Fejl");
            }
            else
            {
                // Update old shelf in shelfRepository
                Shelf oldShelf = _shelfRepository.GetById(SelectedRentalRecord.ShelfNumber);
                _shelfRepository.Update(new Shelf(oldShelf.ShelfNumber, oldShelf.Configuration, Core.Models.Status.Ledig));
                // Update new shelf in shelfRepository
                Shelf newShelf = new Shelf((int)ShelfNumber, _shelfRepository.GetById((int)ShelfNumber).Configuration, Core.Models.Status.Booket);
                _shelfRepository.Update(newShelf);

                // Update rental in repository - Delete and create new because ShelfNumber is PK
                Rental newRental = new Rental((int)ShelfNumber, DateTime.Today, SelectedRentalRecord.RenterId, SelectedRentalRecord.UserId);
                _rentalRepository.Delete(SelectedRentalRecord.ShelfNumber);
                _rentalRepository.Add(newRental);

                // Update RentalRecord in RentalRecords - Remove old and add new
                // Create new record from SelectedRentalRecord with updated properties for Shelf and StartDate
                RentalRecord updatedRecord = SelectedRentalRecord with { ShelfNumber = newShelf.ShelfNumber, Configuration = newShelf.Configuration, Status = newShelf.Status, StartDate = DateTime.Today};
                // Remove old and add new RentalRecord from RentalRecords
                RentalRecords.Remove(SelectedRentalRecord);
                RentalRecords.Add(updatedRecord);

                // Update SelectedRentalRecord to show updated Properties
                SelectedRentalRecord = updatedRecord;
            }
        }

        // -----  CanExecute commands  -----

        // CanUpdate when ShelfNumber, SelectedRental != null
        private bool CanUpdate()
        {
            if (SelectedRentalRecord == null || ShelfNumber == null)
            {
                return false;
            }
            return true;
        }

        // CanTerminate when SelectedRental != null
        private bool CanTerminate()
        {
            if (SelectedRentalRecord == null)
            {
                return false;
            }
            return true;
        }

        // -----  public commands  -----

        // Fetches every shelf from the database.
        // Made public so MainViewModel can refresh the list when navigating here.
        public void LoadRentalRecords()
        {
            RentalRecords.Clear();
            var rentalsFromDb = _rentalRepository.GetAll();
            foreach (var rental in rentalsFromDb)
            {
                var rentalShelf = _shelfRepository.GetById(rental.ShelfNumber);
                var rentalRenter = _renterRepository.GetById(rental.RenterId);
                RentalRecords.Add(new RentalRecord(rental.RenterId, rental.UserId ,rental.StartDate,rentalRenter.FirstName, rentalRenter.LastName, rentalRenter.PhoneNumber, rental.ShelfNumber, rentalShelf.Configuration, rentalShelf.Status));
            }
        }
    }
}

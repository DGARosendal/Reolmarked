using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Models;
using Reolmarked.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Reolmarked.UI.ViewModels
{
    public class ManageRentalViewModel : ViewModelBase
    {
        // ----- Repositories -----
        private readonly IRentalRepository _rentalRepository;
        private readonly IShelfRepository _shelfRepository;
        private readonly IShelfRenterRepository _renterRepository;

        // ----- For DataGrid -----
        // Collection of Rentals (Now includes RentalId and EndDate)
        public record RentalRecord(
            int RentalId,
            int RenterId,
            DateTime StartDate,
            DateTime? EndDate,
            string FirstName,
            string LastName,
            string PhoneNumber,
            int ShelfNumber,
            ShelfConfiguration ShelfConfigurationChoosen,
            Status Status
        );

        public ObservableCollection<RentalRecord> RentalRecords { get; set; }

        // Selected Rental
        private RentalRecord? _selectedRentalRecord;
        public RentalRecord? SelectedRentalRecord
        {
            get => _selectedRentalRecord;
            set
            {
                if (SetField(ref _selectedRentalRecord, value))
                {
                    if (_selectedRentalRecord != null)
                    {
                        // Update Properties
                        StartDate = _selectedRentalRecord.StartDate.ToString("d");
                        EndDate = _selectedRentalRecord.EndDate?.ToString("d") ?? "Aktiv";
                        FirstName = _selectedRentalRecord.FirstName;
                        LastName = _selectedRentalRecord.LastName;
                        PhoneNumber = _selectedRentalRecord.PhoneNumber;
                        ShelfNumber = _selectedRentalRecord.ShelfNumber;
                        ShelfConfigurationChoosen = _selectedRentalRecord.ShelfConfigurationChoosen;
                        Status = _selectedRentalRecord.Status;
                    }
                    else
                    {
                        ClearSelection();
                    }
                    UpdateCommand?.RaiseCanExecuteChanged();
                    TerminateCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        // ----- Properties for textboxes -----
        private string _startDate = string.Empty;
        public string StartDate
        {
            get => _startDate;
            set => SetField(ref _startDate, value);
        }

        private string _endDate = string.Empty;
        public string EndDate
        {
            get => _endDate;
            set => SetField(ref _endDate, value);
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

        private ShelfConfiguration? _shelfConfigurationChoosen;
        public ShelfConfiguration? ShelfConfigurationChoosen
        {
            get => _shelfConfigurationChoosen;
            set => SetField(ref _shelfConfigurationChoosen, value);
        }

        // ----- RelayCommands -----
        public RelayCommand UpdateCommand { get; set; }
        public RelayCommand TerminateCommand { get; set; }
        public RelayCommand ClearSelectionCommand { get; set; }

        // ----- ctor with repositories -----
        public ManageRentalViewModel(
            IRentalRepository rentalRepository,
            IShelfRepository shelfRepository,
            IShelfRenterRepository renterRepository)
        {
            _rentalRepository = rentalRepository;
            _shelfRepository = shelfRepository;
            _renterRepository = renterRepository;

            UpdateCommand = new RelayCommand(Update, CanUpdate);
            TerminateCommand = new RelayCommand(Terminate, CanTerminate);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            RentalRecords = new ObservableCollection<RentalRecord>();

            LoadRentalRecords();
        }

        // ----- Execute commands -----

        private void ClearSelection(object? parameter = null)
        {
            StartDate = string.Empty;
            EndDate = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            ShelfNumber = null;
            ShelfConfigurationChoosen = null;
            Status = null;
            SelectedRentalRecord = null;
        }

        

        // Terminate rental by calculating EndDate and updating RentalId
        private void Terminate(object? parameter = null)
        {
            if (SelectedRentalRecord == null) return;

            try
            {
                // 1. Calculate termination date on the model instance
                Rental rental = new Rental(
                    SelectedRentalRecord.RentalId,
                    SelectedRentalRecord.ShelfNumber,
                    SelectedRentalRecord.StartDate,
                    SelectedRentalRecord.EndDate,
                    SelectedRentalRecord.RenterId
                );
                rental.SetTerminationDate();

                string displayDate = rental.EndDate?.ToString("d") ?? "Ingen slutdato";
                string terminateMessage = $"Opsig reol {SelectedRentalRecord.ShelfNumber} til d. {displayDate}?";

                MessageBoxResult messageResult = MessageBox.Show(terminateMessage, "Opsig reol", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageResult == MessageBoxResult.Yes)
                {
                    // 2. Save updated EndDate to SQL Server for matching RentalId
                    _rentalRepository.Update(rental);

                    // 3. Set shelf status to "Opsagt"
                    Shelf? shelf = _shelfRepository.GetById(SelectedRentalRecord.ShelfNumber);
                    if (shelf != null)
                    {
                        shelf.Status = Core.Models.Status.Opsagt;
                        _shelfRepository.Update(shelf);
                    }

                    // 4. Update local record in ObservableCollection
                    RentalRecord updatedRecord = SelectedRentalRecord with
                    {
                        EndDate = rental.EndDate,
                        Status = Core.Models.Status.Opsagt
                    };

                    int index = RentalRecords.IndexOf(SelectedRentalRecord);
                    if (index >= 0)
                    {
                        RentalRecords[index] = updatedRecord;
                    }

                    SelectedRentalRecord = updatedRecord;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke opsige reol: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Update(object? parameter = null)
        {
            // 1. Basic Null Guard
            if (ShelfNumber == null || SelectedRentalRecord == null) return;

            try
            {
                Shelf? targetShelf = _shelfRepository.GetById((int)ShelfNumber);
                if (targetShelf == null || targetShelf.Status != Core.Models.Status.Ledig)
                {
                    MessageBox.Show($"Reol nr. {(int)ShelfNumber} er ikke ledig. Vælg en anden.", "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

            // 3. Create newRental UPFRONT to test business rules BEFORE database updates
            Rental newRental = new Rental(
                targetShelf.ShelfNumber,
                SelectedRentalRecord.StartDate,
                SelectedRentalRecord.RenterId,
                SelectedRentalRecord.EndDate
            );

            if (newRental.isTerminated())
            {
                MessageBox.Show($"Reollejer har allerede opsagt reol. Vælg en anden.", "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Exits safely without polluting the database!
            }

            // --- ALL CHECKS PASSED: SAFE TO MODIFY DATABASE BELOW ---

            // 4. Free up old shelf
            Shelf oldShelf = _shelfRepository.GetById(SelectedRentalRecord.ShelfNumber);
            if (oldShelf != null)
            {
                oldShelf.Status = Core.Models.Status.Ledig;
                _shelfRepository.Update(oldShelf);
            }

            // 5. Update target shelf
            targetShelf.Status = Core.Models.Status.Booket;
            _shelfRepository.Update(targetShelf);

            // 6. Replace Rental record in DB
            _rentalRepository.Delete(SelectedRentalRecord.RentalId);
            _rentalRepository.Add(newRental); // Ensure Add() populates newRental.RentalId with SCOPE_IDENTITY()

            // 7. Update UI Collection
            int index = RentalRecords.IndexOf(SelectedRentalRecord);
            if (index < 0)
            {
                index = RentalRecords.ToList().FindIndex(r => r.RentalId == SelectedRentalRecord.RentalId);
            }

            if (index >= 0)
            {
                RentalRecord updatedRecord = SelectedRentalRecord with
                {
                    RentalId = newRental.RentalId,
                    ShelfNumber = targetShelf.ShelfNumber,
                    ShelfConfigurationChoosen = targetShelf.ShelfConfiguration,
                    Status = targetShelf.Status,
                    StartDate = newRental.StartDate,
                    EndDate = null
                };

                int index = RentalRecords.IndexOf(SelectedRentalRecord);
                if (index >= 0)
                {
                    RentalRecords[index] = updatedRecord;
                }
        }

        private bool CanUpdate(object? parameter) => SelectedRentalRecord != null && ShelfNumber != null;

        private bool CanTerminate(object? parameter) => SelectedRentalRecord != null;

        public void LoadRentalRecords()
        {
            try
            {
                RentalRecords.Clear();
                var rentalsFromDb = _rentalRepository.GetAll();
                foreach (var rental in rentalsFromDb)
                {
                    var rentalShelf = _shelfRepository.GetById(rental.ShelfNumber);
                    var rentalRenter = _renterRepository.GetById(rental.RenterId);

                    RentalRecords.Add(new RentalRecord(
                        rental.RentalId,
                        rental.RenterId,
                        rental.StartDate,
                        rental.EndDate,
                        rentalRenter?.FirstName ?? "",
                        rentalRenter?.LastName ?? "",
                        rentalRenter?.PhoneNumber ?? "",
                        rental.ShelfNumber,
                        rentalShelf?.ShelfConfiguration ?? ShelfConfiguration.SeksHylder,
                        rentalShelf?.Status ?? Core.Models.Status.Ledig
                    ));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke hente udlejninger: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
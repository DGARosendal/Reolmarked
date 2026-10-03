// SRP: This ViewModel is responsible for the "ReolLejer" screen (UC-2).

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.Commands;

namespace Reolmarked.UI.ViewModels
{
    public class ShelfRenterViewModel : ViewModelBase
    {
        private readonly IShelfRenterRepository _renterRepository;

        public ObservableCollection<ShelfRenter> ShelfRenters { get; }

        private ShelfRenter? _selectedRenter;
        public ShelfRenter? SelectedRenter
        {
            get => _selectedRenter;
            set
            {
                if (SetField(ref _selectedRenter, value))
                {
                    if (_selectedRenter != null)
                    {
                        FirstName = _selectedRenter.FirstName;
                        LastName = _selectedRenter.LastName;
                        PhoneNumber = _selectedRenter.PhoneNumber;
                    }
                    UpdateCommand?.RaiseCanExecuteChanged();
                    DeleteCommand?.RaiseCanExecuteChanged();
                }
            }
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

        public RelayCommand AddCommand { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearSelectionCommand { get; }

        public ShelfRenterViewModel() : this(new ShelfRenterRepository()) { }

        public ShelfRenterViewModel(IShelfRenterRepository renterRepository)
        {
            _renterRepository = renterRepository;
            ShelfRenters = new ObservableCollection<ShelfRenter>();

            AddCommand = new RelayCommand(AddRenter);
            UpdateCommand = new RelayCommand(UpdateRenter, CanUpdate);
            DeleteCommand = new RelayCommand(DeleteRenter, CanDelete);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            LoadRenters();
        }

        // Fetches every renter from the database.
        // Made public so MainViewModel can refresh the list when navigating here.
        public void LoadRenters()
        {
            ShelfRenters.Clear();
            var rentersFromDb = _renterRepository.GetAll();
            foreach (var renter in rentersFromDb)
            {
                ShelfRenters.Add(renter);
            }
        }

        private void ClearSelection(object? parameter)
        {
            ClearForm();
        }

        private bool CanUpdate(object? parameter) => SelectedRenter != null;
        private bool CanDelete(object? parameter) => SelectedRenter != null;

        private void AddRenter(object? parameter)
        {
            try
            {
                // Check if phone number already exists in database/collection.
                if (ShelfRenters.Any(r => r.PhoneNumber.Equals(PhoneNumber?.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"En anden reollejer med telefonnummer '{PhoneNumber}' eksisterer allerede.");
                }

                var newRenter = new ShelfRenter(FirstName, LastName, PhoneNumber);
                _renterRepository.Add(newRenter);
                ShelfRenters.Add(newRenter);
                SelectedRenter = newRenter;
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke tilføje reollejer: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateRenter(object? parameter)
        {
            try
            {
                if (SelectedRenter == null) return;

                // Check if another renter already uses the target phone number.
                if (ShelfRenters.Any(r => r.RenterId != SelectedRenter.RenterId &&
                                     r.PhoneNumber.Equals(PhoneNumber?.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"En anden reollejer med telefonnummer '{PhoneNumber}' eksisterer allerede.");
                }

                // 1. Update properties on the selected object.
                SelectedRenter.FirstName = FirstName;
                SelectedRenter.LastName = LastName;
                SelectedRenter.PhoneNumber = PhoneNumber;

                // 2. Persist to database.
                _renterRepository.Update(SelectedRenter);

                // 3. Replace the item at its index to trigger DataGrid UI update.
                int index = ShelfRenters.IndexOf(SelectedRenter);
                if (index >= 0)
                {
                    ShelfRenters[index] = new ShelfRenter(
                        SelectedRenter.RenterId,
                        SelectedRenter.FirstName,
                        SelectedRenter.LastName,
                        SelectedRenter.PhoneNumber
                    );
                    SelectedRenter = ShelfRenters[index];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke opdatere reollejer: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteRenter(object? parameter)
        {
            try
            {
                if (SelectedRenter == null) return;

                var result = MessageBox.Show(
                    $"Er du sikker på at du vil slette {SelectedRenter.FirstName} {SelectedRenter.LastName}?",
                    "Bekræft sletning", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                _renterRepository.Delete(SelectedRenter.RenterId);
                ShelfRenters.Remove(SelectedRenter);
                LoadRenters();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke slette reollejer: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            SelectedRenter = null;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
        }
    }
}
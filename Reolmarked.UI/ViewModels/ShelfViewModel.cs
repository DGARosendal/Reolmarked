using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Reolmarked.UI.ViewModels
{
    public class ShelfViewModel : ViewModelBase
    {
        private readonly IShelfRepository _shelfRepository;

        public ObservableCollection<Shelf> Shelves { get; }

        // Dropdown options populated directly from the enum definitions
        public List<Status> StatusOptions { get; } = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
        public List<Configuration> ConfigurationOptions { get; } = Enum.GetValues(typeof(Configuration)).Cast<Configuration>().ToList();

        private Shelf? _selectedShelf;
        public Shelf? SelectedShelf
        {
            get => _selectedShelf;
            set
            {
                if (SetField(ref _selectedShelf, value))
                {
                    if (_selectedShelf != null)
                    {
                        ShelfNumber = _selectedShelf.ShelfNumber;
                        Status = _selectedShelf.Status;
                        Configuration = _selectedShelf.Configuration;
                    }
                    UpdateCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private int _shelfNumber;
        public int ShelfNumber
        {
            get => _shelfNumber;
            set => SetField(ref _shelfNumber, value);
        }

        private Status _status = Status.Ledig;
        public Status Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        private Configuration _configuration = Configuration.SeksHylder;
        public Configuration Configuration
        {
            get => _configuration;
            set => SetField(ref _configuration, value);
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SelectShelfCommand { get; }

        public ShelfViewModel() : this(new ShelfRepository())
        {
        }

        public ShelfViewModel(IShelfRepository shelfRepository)
        {
            _shelfRepository = shelfRepository;
            Shelves = new ObservableCollection<Shelf>();

            AddCommand = new RelayCommand(AddShelf);
            UpdateCommand = new RelayCommand(UpdateShelf, CanUpdate);
            DeleteCommand = new RelayCommand(DeleteShelf, CanDelete);
            SelectShelfCommand = new RelayCommand(SelectShelf);

            LoadShelves();
        }

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

        private void SelectShelf(object? parameter)
        {
            if (parameter is Shelf shelf)
            {
                SelectedShelf = shelf;
            }
        }

        private bool CanUpdate(object? parameter)
        {
            return SelectedShelf != null;
        }

        private bool CanDelete(object? parameter)
        {
            return Shelves.Any();
        }

        private void AddShelf(object? parameter)
        {
            try
            {
                // Create new shelf with temporary number 1 (DB auto-assigns real ID via SCOPE_IDENTITY)
                var newShelf = new Shelf(1, Configuration, Status);

                _shelfRepository.Add(newShelf);
                Shelves.Add(newShelf);
                SelectedShelf = newShelf;

                DeleteCommand.RaiseCanExecuteChanged();

            }

            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke tilføje reol: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateShelf(object? parameter)
        {
            try
            {
                if (SelectedShelf == null) return;

                SelectedShelf.Status = Status;
                SelectedShelf.Configuration = Configuration;

                _shelfRepository.Update(SelectedShelf);

                // Refresh item in list so binding/converter reflects updated status
                int index = Shelves.IndexOf(SelectedShelf);
                if (index >= 0)
                {
                    Shelves[index] = null;
                    Shelves[index] = SelectedShelf;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke opdatere reol: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteShelf(object? parameter)
        {
            try
            {
                var lastShelf = Shelves.LastOrDefault();
                if (lastShelf == null) return;

                var result = MessageBox.Show(
                    $"Er du sikker på, at du vil slette den sidste reol (Reol nr. {lastShelf.ShelfNumber})?",
                    "Bekræft sletning", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                _shelfRepository.Delete(lastShelf.ShelfNumber);
                Shelves.Remove(lastShelf);

                if (SelectedShelf == lastShelf)
                {
                    ClearSelection();
                }

                DeleteCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunne ikke slette reol: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSelection()
        {
            SelectedShelf = null;
            ShelfNumber = 0;
            Status = Status.Ledig;
            Configuration = Configuration.SeksHylder;
        }
    }
}
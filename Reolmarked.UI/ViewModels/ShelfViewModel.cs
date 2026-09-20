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

        public List<string> StatusOptions { get; } = new List<string> { "Grøn", "Gul", "Rød" };
        public List<string> ConfigurationOptions { get; } = new List<string> { "Reol", "Bøjle" };

        private Shelf _selectedShelf;
        public Shelf SelectedShelf
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

        private string _status = "Grøn";
        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        private string _configuration = "Reol";
        public string Configuration
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

        private void LoadShelves()
        {
            Shelves.Clear();
            var shelvesFromDb = _shelfRepository.GetAll();
            foreach (var shelf in shelvesFromDb)
            {
                Shelves.Add(shelf);
            }
        }

        private void SelectShelf(object parameter)
        {
            if (parameter is Shelf shelf)
            {
                SelectedShelf = shelf;
            }
        }

        private bool CanUpdate(object parameter)
        {
            return SelectedShelf != null && !string.IsNullOrWhiteSpace(Status) && !string.IsNullOrWhiteSpace(Configuration);
        }

        // Delete button is enabled as long as there is at least one shelf in the list
        private bool CanDelete(object parameter)
        {
            return Shelves.Any();
        }

        private void AddShelf(object parameter)
        {
            try
            {
                var newShelf = new Shelf
                {
                    Status = Status ?? "Grøn",
                    Configuration = Configuration ?? "Reol"
                };

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

        private void UpdateShelf(object parameter)
        {
            try
            {
                if (SelectedShelf == null) return;

                SelectedShelf.Status = Status;
                SelectedShelf.Configuration = Configuration;

                _shelfRepository.Update(SelectedShelf);

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

        private void DeleteShelf(object parameter)
        {
            try
            {
                // Grab the last shelf in the list
                var lastShelf = Shelves.LastOrDefault();
                if (lastShelf == null) return;

                var result = MessageBox.Show(
                    $"Er du sikker på, at du vil slette den sidste reol (Reol nr. {lastShelf.ShelfNumber})?",
                    "Bekræft sletning", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                // Delete last shelf from database and UI list
                _shelfRepository.Delete(lastShelf.ShelfNumber);
                Shelves.Remove(lastShelf);

                // If the deleted shelf was selected, clear input fields
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
            Status = "Grøn";
            Configuration = "Reol";
        }
    }
}
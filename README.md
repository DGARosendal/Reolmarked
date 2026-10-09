# Reolmarked

# Reolmarked - Design Class Diagrams (DCD)

Dette dokument indeholder projektets to versioner af DCD'et i overensstemmelse med projektkravet: Én model der viser sporbarhed til domænemodellen, og én model der viser eksempler på arkitekturens lag-samspil.

## DCD med sporbarhed til DM (Modellaget)
Dette diagram viser klasserne og enumerations i domæne-/modellaget[cite: 1].

```mermaid
classDiagram
    namespace Reolmarked_Core_Models {
        class Configuration {
            <<enumeration>>
            TreHylderOgBøjle
            SeksHylder
        }

        class Status {
            <<enumeration>>
            Ledig
            Booket
            Opsagt
            UdeAfDrift
        }

        class Shelf {
            - _shelfNumber: int
            - _configuration: Configuration
            - _status: Status
            + ShelfNumber: int
            + Configuration: Configuration
            + Status: Status
            + Shelf(shelfNumber: int, configuration: Configuration, status: Status)
        }

        class ShelfRenter {
            - MaxNameLength: int
            - MaxPhoneNumberLength: int
            - _firstName: string
            - _lastName: string
            - _phoneNumber: string
            - _balance: double
            + RenterId: int
            + FirstName: string
            + LastName: string
            + PhoneNumber: string
            + Balance: double
            + ShelfRenter(renterId: int, firstName: string, lastName: string, phoneNumber: string, balance: double)
            + ShelfRenter(firstName: string, lastName: string, phoneNumber: string)
            - ValidateName(name: string, fieldName: string) string
            - ValidatePhoneNumber(phoneNumber: string) string
        }

        class Rental {
            - _shelfNumber: int
            - _startDate: DateTime
            - _monthlyRent: double
            - _renterId: int
            - _userId: int
            + ShelfNumber: int
            + StartDate: DateTime
            + MonthlyRent: double
            + RenterId: int
            + UserId: int
            + Rental(shelfNumber: int, startDate: DateTime, monthlyRent: double, renterId: int, userId: int)
        }

        class MonthlySettlement {
            - _settlementId: int
            - _renterId: int
            - _month: int
            - _totalSales: double
            - _commission: double
            - _totalShelfRent: double
            - _shelfCount: int
            - _extraDiscount: double
            - _finalAmount: double
            - _isProcessed: bool
            + SettlementId: int
            + RenterId: int
            + Month: int
            + TotalSales: double
            + Commission: double
            + TotalShelfRent: double
            + ShelfCount: int
            + ExtraDiscount: double
            + FinalAmount: double
            + IsProcessed: bool
            + MonthlySettlement(settlementId: int, renterId: int, month: int, totalSales: double, commission: double, totalShelfRent: double, shelfCount: int, extraDiscount: double, finalAmount: double, isProcessed: bool)
        }
    }

    Shelf "1" -- "1" Configuration
    Shelf "1" -- "1" Status
    Rental "0..*" -- "1" Shelf
    Rental "1..*" -- "1" ShelfRenter
    MonthlySettlement "*" -- "1" ShelfRenter
```
## Reolmarked - Arkitektur DCD (Rental Eksempel)

Dette diagram viser et komplet eksempel på tværs af lagene (Modellag, Repository / Data Access og Præsentation / ViewModel) med fokus på **Rental**-domænet.

```mermaid
classDiagram
    namespace Reolmarked_Core_Models {
        class ShelfRenter {
            - MaxNameLength: int
            - MaxPhoneNumberLength: int
            - _firstName: string
            - _lastName: string
            - _phoneNumber: string
            - _balance: double
            + RenterId: int
            + FirstName: string
            + LastName: string
            + PhoneNumber: string
            + Balance: double
            + ShelfRenter(renterId: int, firstName: string, lastName: string, phoneNumber: string, balance: double)
            + ShelfRenter(firstName: string, lastName: string, phoneNumber: string)
            - ValidateName(name: string, fieldName: string) string
            - ValidatePhoneNumber(phoneNumber: string) string
        }
    }

    namespace Reolmarked_Core_Repositories {
        class IShelfRenterRepository {
            <<interface>>
            + Add(renter: ShelfRenter) void
            + Update(renter: ShelfRenter) void
            + Delete(renterId: int) void
            + GetAll() List~ShelfRenter~
            + GetById(renterId: int) ShelfRenter
        }

        class ShelfRenterRepository {
            - _connectionString: string
            + ShelfRenterRepository()
            + Add(renter: ShelfRenter) void
            + Update(renter: ShelfRenter) void
            + Delete(renterId: int) void
            + GetAll() List~ShelfRenter~
            + GetById(renterId: int) ShelfRenter
            - MapRenter(reader: SqlDataReader) ShelfRenter
        }
    }

    namespace Reolmarked_UI_ViewModels {
        class ShelfRenterViewModel {
            - _renterRepository: IShelfRenterRepository
            - _selectedRenter: ShelfRenter
            - _firstName: string
            - _lastName: string
            - _phoneNumber: string
            - _balance: double
            + ShelfRenters: ObservableCollection~ShelfRenter~
            + SelectedRenter: ShelfRenter
            + FirstName: string
            + LastName: string
            + PhoneNumber: string
            + Balance: double
            + AddCommand: RelayCommand
            + UpdateCommand: RelayCommand
            + DeleteCommand: RelayCommand
            + ClearSelectionCommand: RelayCommand
            + OpenMonthlySettlementCommand: RelayCommand
            + ShelfRenterViewModel()
            + ShelfRenterViewModel(renterRepository: IShelfRenterRepository)
            - ClearSelection(parameter: object) void
            - LoadRenters() void
            - CanUpdate(parameter: object) bool
            - CanDelete(parameter: object) bool
            - AddRenter(parameter: object) void
            - UpdateRenter(parameter: object) void
            - DeleteRenter(parameter: object) void
            - ClearForm() void
        }
    }

    IShelfRenterRepository <|.. ShelfRenterRepository : implementerer
    ShelfRenterViewModel "1" -- "1" IShelfRenterRepository : afhænger af 
    ShelfRenterViewModel "1" o-- "*" ShelfRenter : binder til
    ShelfRenterRepository "1" o-- "*" ShelfRenter : indeholder
```

## Fuld DCD for UC3
```mermaid
classDiagram

    %% ---------------------------------------------------------
    %% DOMAIN: Reolmarked.Core.Models (Domain Models)
    %% ---------------------------------------------------------
    namespace Reolmarked_Core_Models {
        class Configuration {
            <<enumeration>>
            TreHylderOgBøjle
            SeksHylder
        }

        class Status {
            <<enumeration>>
            Ledig
            Booket
            Opsagt
            UdeAfDrift
        }

        class Shelf {
            - _shelfNumber: int
            - _configuration: Configuration
            - _status: Status
            + ShelfNumber: int
            + Configuration: Configuration
            + Status: Status
            + Shelf(shelfNumber: int, configuration: Configuration, status: Status)
        }

        class ShelfRenter {
            - MaxNameLength: int
            - MaxPhoneNumberLength: int
            - _firstName: string
            - _lastName: string
            - _phoneNumber: string
            + RenterId: int
            + FirstName: string
            + LastName: string
            + PhoneNumber: string
            + ShelfRenter(renterId: int, firstName: string, lastName: string, phoneNumber: string)
            + ShelfRenter(firstName: string, lastName: string, phoneNumber: string)
            - ValidateName(name: string, fieldName: string) string
            - ValidatePhoneNumber(phoneNumber: string) string
        }

        class Rental {
            - _shelfNumber: int
            - _startDate: DateTime
            - _monthlyRent: double
            - _renterId: int
            - _userId: int
            + ShelfNumber: int
            + StartDate: DateTime
            + MonthlyRent: double
            + RenterId: int
            + UserId: int
            + Rental(shelfNumber: int, startDate: DateTime, monthlyRent: double, renterId: int, userId: int)
        }
    }

    %% ---------------------------------------------------------
    %% DOMAIN: Reolmarked.Core.Repositories (Data Access Layer)
    %% ---------------------------------------------------------
    namespace Reolmarked_Core_Repositories {
        class IShelfRepository {
            <<interface>>
            + Add(shelf: Shelf) void
            + Update(shelf: Shelf) void
            + Delete(shelfNumber: int) void
            + GetAll() List~Shelf~
            + GetById(shelfNumber: int) Shelf
        }

        class IShelfRenterRepository {
            <<interface>>
            + Add(renter: ShelfRenter) void
            + Update(renter: ShelfRenter) void
            + Delete(renterId: int) void
            + GetAll() List~ShelfRenter~
            + GetById(renterId: int) ShelfRenter
        }

        class IRentalRepository {
            <<interface>>
            + Add(rental: Rental) void
            + Update(rental: Rental) void
            + Delete(shelfNumber: int) void
            + GetAll() List~Rental~
            + GetByShelfNumber(shelfNumber: int) Rental
            + GetByRenterId(renterId: int) List~Rental~
        }

        class ShelfRepository {
            - _connectionString: string
            + ShelfRepository()
            + Add(shelf: Shelf) void
            + Update(shelf: Shelf) void
            + Delete(shelfNumber: int) void
            + GetAll() List~Shelf~
            + GetById(shelfNumber: int) Shelf
            - MapShelf(reader: SqlDataReader) Shelf
        }

        class ShelfRenterRepository {
            - _connectionString: string
            + ShelfRenterRepository()
            + Add(renter: ShelfRenter) void
            + Update(renter: ShelfRenter) void
            + Delete(renterId: int) void
            + GetAll() List~ShelfRenter~
            + GetById(renterId: int) ShelfRenter
            - MapRenter(reader: SqlDataReader) ShelfRenter
        }

        class RentalRepository {
            - _connectionString: string
            + RentalRepository()
            + Add(rental: Rental) void
            + Update(rental: Rental) void
            + Delete(shelfNumber: int) void
            + GetAll() List~Rental~
            + GetByShelfNumber(shelfNumber: int) Rental
            + GetByRenterId(renterId: int) List~Rental~
            - MapRental(reader: SqlDataReader) Rental
        }
    }

  
    %% ---------------------------------------------------------
    %% DOMAIN: Reolmarked.UI.ViewModels (Presentation Layer)
    %% ---------------------------------------------------------
    namespace Reolmarked_UI_ViewModels {
        class ViewModelBase {
            + event PropertyChanged: PropertyChangedEventHandler
            # OnPropertyChanged(name: string) void
            # SetField~T~(field: ref T, value: T, propertyName: string) bool
        }

        class MainViewModel {
            - _currentView: object
            + CurrentView: object
            + ShelfViewCommand: RelayCommand
            + ShelfRenterViewCommand: RelayCommand
            + RentalViewCommand: RelayCommand
            + ShelfVM: ShelfViewModel
            + ShelfRenterVM: ShelfRenterViewModel
            + RentalVMCR: RentalViewModel
            + RentalVMUD: ManageRentalViewModel
            + MainViewModel()
        }

        class ShelfViewModel {
            - _shelfRepository: IShelfRepository
            - _selectedShelf: Shelf
            - _shelfNumber: int
            - _status: Status
            - _configuration: Configuration
            + Shelves: ObservableCollection~Shelf~
            + StatusOptions: List~Status~
            + ConfigurationOptions: List~Configuration~
            + SelectedShelf: Shelf
            + ShelfNumber: int
            + Status: Status
            + Configuration: Configuration
            + AddCommand: RelayCommand
            + UpdateCommand: RelayCommand
            + DeleteCommand: RelayCommand
            + SelectShelfCommand: RelayCommand
            + ShelfViewModel()
            + ShelfViewModel(shelfRepository: IShelfRepository)
            - LoadShelves() void
            - SelectShelf(parameter: object) void
            - CanUpdate(parameter: object) bool
            - CanDelete(parameter: object) bool
            - AddShelf(parameter: object) void
            - UpdateShelf(parameter: object) void
            - DeleteShelf(parameter: object) void
            - ClearSelection() void
        }

        class ShelfRenterViewModel {
            - _renterRepository: IShelfRenterRepository
            - _selectedRenter: ShelfRenter
            - _firstName: string
            - _lastName: string
            - _phoneNumber: string
            + ShelfRenters: ObservableCollection~ShelfRenter~
            + SelectedRenter: ShelfRenter
            + FirstName: string
            + LastName: string
            + PhoneNumber: string
            + AddCommand: RelayCommand
            + UpdateCommand: RelayCommand
            + DeleteCommand: RelayCommand
            + ClearSelectionCommand: RelayCommand
            + ShelfRenterViewModel()
            + ShelfRenterViewModel(renterRepository: IShelfRenterRepository)
            - ClearSelection(parameter: object) void
            - LoadRenters() void
            - CanUpdate(parameter: object) bool
            - CanDelete(parameter: object) bool
            - AddRenter(parameter: object) void
            - UpdateRenter(parameter: object) void
            - DeleteRenter(parameter: object) void
            - ClearForm() void
        }

      class RentalViewModel {
    - _rentalRepository: IRentalRepository
    - _shelfRepository: IShelfRepository
    - _renterRepository: IShelfRenterRepository
    - _selectedShelf: Shelf?
    - _searchPhoneNumber: string
    - _selectedRenter: ShelfRenter?
    - _newFirstName: string
    - _newLastName: string
    - _newPhoneNumber: string
    - _startDate: DateTime
    - _isConfirmationVisible: bool
    + Shelves: ObservableCollection~Shelf~
    + SelectedShelf: Shelf?
    + SearchPhoneNumber: string
    + SearchResults: ObservableCollection~ShelfRenter~
    + SelectedRenter: ShelfRenter?
    + NewFirstName: string
    + NewLastName: string
    + NewPhoneNumber: string
    + StartDate: DateTime
    + IsConfirmationVisible: bool
    + CreateRenterCommand: RelayCommand
    + SelectShelfCommand: RelayCommand
    + BookCommand: RelayCommand
    + ConfirmBookingCommand: RelayCommand
    + CancelBookingCommand: RelayCommand
    + RentalViewModel(rentalRepository: IRentalRepository, shelfRepository: IShelfRepository, renterRepository: IShelfRenterRepository)
    + LoadShelves(): void
    - SearchRenter(parameter: object?): void
    - CreateRenter(parameter: object?): void
    - SelectShelf(parameter: object?): void
    - CanBook(parameter: object?): bool
    - ShowConfirmation(parameter: object?): void
    - ConfirmBooking(parameter: object?): void
    - CancelBooking(parameter: object?): void
}

        class ManageRentalViewModel {
    - _rentalRepository: IRentalRepository
    - _shelfRepository: IShelfRepository
    - _renterRepository: IShelfRenterRepository
    - _selectedRental: Rental?
    - _searchText: string
    - _showOnlyActive: bool
    - _currentRenter: ShelfRenter?
    - _currentShelf: Shelf?
    - _isEndConfirmationVisible: bool
    - _endDate: DateTime
    + Rentals: ObservableCollection~Rental~
    + SelectedRental: Rental?
    + SearchText: string
    + ShowOnlyActive: bool
    + CurrentRenter: ShelfRenter?
    + CurrentShelf: Shelf?
    + IsEndConfirmationVisible: bool
    + EndDate: DateTime
    + EndRentalCommand: RelayCommand
    + ConfirmEndRentalCommand: RelayCommand
    + CancelEndRentalCommand: RelayCommand
    + DeleteRentalCommand: RelayCommand
    + RefreshCommand: RelayCommand
    + ManageRentalViewModel(rentalRepository: IRentalRepository, shelfRepository: IShelfRepository, renterRepository: IShelfRenterRepository)
    + LoadRentals(): void
    - ApplyFilter(): void
    - LoadSelectedDetails(): void
    - CanEndRental(parameter: object?): bool
    - ShowEndConfirmation(parameter: object?): void
    - ConfirmEndRental(parameter: object?): void
    - CancelEndRental(parameter: object?): void
    - DeleteRental(parameter: object?): void
}
    }
     %% ---------------------------------------------------------
    %% DOMAIN: Reolmarked.UI.Commands & Core (Infrastructure)
    %% ---------------------------------------------------------
    namespace Reolmarked_UI_Infrastructure {
        class RelayCommand {
            - _execute: Action~object~
            - _canExecute: Func~object, bool~
            + event CanExecuteChanged: EventHandler
            + RelayCommand(execute: Action~object~, canExecute: Func~object, bool~)
            + RelayCommand(execute: Action, canExecute: Func~bool~)
            + CanExecute(parameter: object) bool
            + Execute(parameter: object) void
            + RaiseCanExecuteChanged() void
        }
    }


    %% ---------------------------------------------------------
    %% RELATIONSHIPS & INHERITANCE
    %% ---------------------------------------------------------

    %% Inheritance (Generalization)
    ViewModelBase <|-- MainViewModel
    ViewModelBase <|-- ShelfViewModel
    ViewModelBase <|-- ShelfRenterViewModel
    ViewModelBase <|-- RentalViewModel
    ViewModelBase <|-- ManageRentalViewModel

    %% Realization (Interface Implementation)
    IShelfRepository <|.. ShelfRepository
    IShelfRenterRepository <|.. ShelfRenterRepository
    IRentalRepository <|.. RentalRepository

    %% Aggregations (Shared Whole-Part with White Diamond o--)
    ShelfViewModel "1" o-- "*" Shelf
    ShelfRenterViewModel "1" o-- "*" ShelfRenter
    RentalViewModel "1" o-- "*" Rental
    ManageRentalViewModel"1" o-- "*" Rental

    MainViewModel "1" -- "1" ShelfViewModel
    MainViewModel "1" -- "1" ShelfRenterViewModel
    MainViewModel "1" -- "1" RentalViewModel
    MainViewModel "1" -- "1" ManageRentalViewModel

    %% Plain Associations (Domain Entities & Dependencies with Multiplicities, No Arrows)
    Shelf "1" -- "1" Configuration
    Shelf "1" -- "1" Status
    Rental "0..*" -- "1" Shelf
    Rental "1..*" -- "1" ShelfRenter

    ShelfRepository "1" o-- "*" Shelf
    ShelfRenterRepository "1" o-- "*" ShelfRenter
    RentalRepository "1" o-- "*" Rental

    IShelfRepository "1" o-- "*" Shelf
    IShelfRenterRepository "1" o-- "*" ShelfRenter
    IRentalRepository "1" o-- "*" Rental

    ShelfViewModel "1" -- "1" IShelfRepository
    ShelfViewModel "1" -- "*" RelayCommand

    ShelfRenterViewModel "1" -- "1" IShelfRenterRepository
    ShelfRenterViewModel "1" -- "*" RelayCommand

    RentalViewModel "1" -- "1" IRentalRepository
    RentalViewModel "1" -- "1" IShelfRepository
    RentalViewModel "1" -- "1" IShelfRenterRepository
    RentalViewModel "1" -- "*" RelayCommand
   
    ManageRentalViewModel "1" -- "1" IRentalRepository
    ManageRentalViewModel "1" -- "1" IShelfRepository
    ManageRentalViewModel "1" -- "1" IShelfRenterRepository
    ManageRentalViewModel "1" -- "*" RelayCommand


    MainViewModel "1" -- "*" RelayCommand

```

## Fuld DCD for UC4
```mermaid
classDiagram

    %% ---------------------------------------------------------
    %% DOMAIN: Reolmarked.Core.Models (Domain Models)
    %% ---------------------------------------------------------
    namespace Reolmarked_Core_Models {
        class Configuration {
            <<enumeration>>
            TreHylderOgBøjle
            SeksHylder
        }

        class Status {
            <<enumeration>>
            Ledig
            Booket
            Opsagt
            UdeAfDrift
        }

        class Shelf {
            - _shelfNumber: int
            - _configuration: Configuration
            - _status: Status
            + ShelfNumber: int
            + Configuration: Configuration
            + Status: Status
            + Shelf(shelfNumber: int, configuration: Configuration, status: Status)
        }

        class ShelfRenter {
            - MaxNameLength: int
            - MaxPhoneNumberLength: int
            - _firstName: string
            - _lastName: string
            - _phoneNumber: string
            - _balance: double
            + RenterId: int
            + FirstName: string
            + LastName: string
            + PhoneNumber: string
            + Balance: double
            + ShelfRenter(renterId: int, firstName: string, lastName: string, phoneNumber: string, balance: double)
            + ShelfRenter(firstName: string, lastName: string, phoneNumber: string)
            - ValidateName(name: string, fieldName: string) string
            - ValidatePhoneNumber(phoneNumber: string) string
        }

        class Rental {
            - _shelfNumber: int
            - _startDate: DateTime
            - _monthlyRent: double
            - _renterId: int
            - _userId: int
            + ShelfNumber: int
            + StartDate: DateTime
            + MonthlyRent: double
            + RenterId: int
            + UserId: int
            + Rental(shelfNumber: int, startDate: DateTime, monthlyRent: double, renterId: int, userId: int)
        }

        class MonthlySettlement {
            - _settlementId: int
            - _renterId: int
            - _month: int
            - _totalSales: double
            - _commission: double
            - _totalShelfRent: double
            - _shelfCount: int
            - _extraDiscount: double
            - _finalAmount: double
            - _isProcessed: bool
            + SettlementId: int
            + RenterId: int
            + Month: int
            + TotalSales: double
            + Commission: double
            + TotalShelfRent: double
            + ShelfCount: int
            + ExtraDiscount: double
            + FinalAmount: double
            + IsProcessed: bool
            + MonthlySettlement(settlementId: int, renterId: int, month: int, totalSales: double, commission: double, totalShelfRent: double, shelfCount: int, extraDiscount: double, finalAmount: double, isProcessed: bool)
        }
    }

    %% ---------------------------------------------------------
    %% DOMAIN: Reolmarked.Core.Repositories (Data Access Layer)
    %% ---------------------------------------------------------
    namespace Reolmarked_Core_Repositories {
        class IShelfRenterRepository {
            <<interface>>
            + Add(renter: ShelfRenter) void
            + Update(renter: ShelfRenter) void
            + Delete(renterId: int) void
            + GetAll() List~ShelfRenter~
            + GetById(renterId: int) ShelfRenter
        }

        class IMonthlySettlementRepository {
            <<interface>>
            + Add(settlement: MonthlySettlement) void
            + Update(settlement: MonthlySettlement) void
            + GetById(settlementId: int) MonthlySettlement
            + GetByRenterId(renterId: int) List~MonthlySettlement~
        }

        class ShelfRenterRepository {
            - _connectionString: string
            + ShelfRenterRepository()
            + Add(renter: ShelfRenter) void
            + Update(renter: ShelfRenter) void
            + Delete(renterId: int) void
            + GetAll() List~ShelfRenter~
            + GetById(renterId: int) ShelfRenter
            - MapRenter(reader: SqlDataReader) ShelfRenter
        }

        class MonthlySettlementRepository {
            - _connectionString: string
            + MonthlySettlementRepository()
            + Add(settlement: MonthlySettlement) void
            + Update(settlement: MonthlySettlement) void
            + GetById(settlementId: int) MonthlySettlement
            + GetByRenterId(renterId: int) List~MonthlySettlement~
            - MapSettlement(reader: SqlDataReader) MonthlySettlement
        }
    }

    %% ---------------------------------------------------------
    %% DOMAIN: Reolmarked.UI.ViewModels (Presentation Layer)
    %% ---------------------------------------------------------
    namespace Reolmarked_UI_ViewModels {
        class ViewModelBase {
            + event PropertyChanged: PropertyChangedEventHandler
            # OnPropertyChanged(name: string) void
            # SetField~T~(field: ref T, value: T, propertyName: string) bool
        }

        class MainViewModel {
            - _currentView: object
            + CurrentView: object
            + ShelfRenterViewCommand: RelayCommand
            + MonthlySettlementViewCommand: RelayCommand
            + ShelfRenterVM: ShelfRenterViewModel
            + MonthlySettlementVM: MonthlySettlementViewModel
            + MainViewModel()
        }

        class ShelfRenterViewModel {
            - _renterRepository: IShelfRenterRepository
            - _selectedRenter: ShelfRenter
            - _firstName: string
            - _lastName: string
            - _phoneNumber: string
            - _balance: double
            + ShelfRenters: ObservableCollection~ShelfRenter~
            + SelectedRenter: ShelfRenter
            + FirstName: string
            + LastName: string
            + PhoneNumber: string
            + Balance: double
            + AddCommand: RelayCommand
            + UpdateCommand: RelayCommand
            + DeleteCommand: RelayCommand
            + ClearSelectionCommand: RelayCommand
            + OpenMonthlySettlementCommand: RelayCommand
            + ShelfRenterViewModel()
            + ShelfRenterViewModel(renterRepository: IShelfRenterRepository)
            - ClearSelection(parameter: object) void
            - LoadRenters() void
            - CanUpdate(parameter: object) bool
            - CanDelete(parameter: object) bool
            - AddRenter(parameter: object) void
            - UpdateRenter(parameter: object) void
            - DeleteRenter(parameter: object) void
            - ClearForm() void
        }

        class MonthlySettlementViewModel {
            - _settlementRepository: IMonthlySettlementRepository
            - _renterRepository: IShelfRenterRepository
            - _selectedSettlement: MonthlySettlement
            - _selectedRenter: ShelfRenter
            - _isProcessed: bool
            - _extraDiscount: double
            + CurrentSettlement: MonthlySettlement
            + CurrentRenter: ShelfRenter
            + TotalSales: double
            + Commission: double
            + TotalShelfRent: double
            + FinalAmount: double
            + IsProcessed: bool
            + ExtraDiscount: double
            + BackCommand: RelayCommand
            + ApproveCommand: RelayCommand
            + MonthlySettlementViewModel(settlementRepository: IMonthlySettlementRepository, renterRepository: IShelfRenterRepository)
            - LoadSettlementForRenter(renterId: int) void
            - CalculateFinalAmount() void
            - ApproveSettlement(parameter: object) void
            - CanApprove(parameter: object) bool
        }
    }

    %% ---------------------------------------------------------
    %% DOMAIN: Reolmarked.UI.Infrastructure
    %% ---------------------------------------------------------
    namespace Reolmarked_UI_Infrastructure {
        class RelayCommand {
            - _execute: Action~object~
            - _canExecute: Func~object, bool~
            + event CanExecuteChanged: EventHandler
            + RelayCommand(execute: Action~object~, canExecute: Func~object, bool~)
            + RelayCommand(execute: Action, canExecute: Func~bool~)
            + CanExecute(parameter: object) bool
            + Execute(parameter: object) void
            + RaiseCanExecuteChanged() void
        }
    }

    %% ---------------------------------------------------------
    %% RELATIONSHIPS & INHERITANCE
    %% ---------------------------------------------------------

    %% Inheritance (Generalization)
    ViewModelBase <|-- MainViewModel
    ViewModelBase <|-- ShelfRenterViewModel
    ViewModelBase <|-- MonthlySettlementViewModel

    %% Realization (Interface Implementation)
    IShelfRenterRepository <|.. ShelfRenterRepository
    IMonthlySettlementRepository <|.. MonthlySettlementRepository

    %% Aggregations
    ShelfRenterViewModel "1" o-- "*" ShelfRenter
    MonthlySettlementViewModel "1" o-- "*" MonthlySettlement

    MainViewModel "1" -- "1" ShelfRenterViewModel
    MainViewModel "1" -- "1" MonthlySettlementViewModel

    %% Associations
    Shelf "1" -- "1" Configuration
    Shelf "1" -- "1" Status
    Rental "0..*" -- "1" Shelf
    Rental "1..*" -- "1" ShelfRenter
    MonthlySettlement "*" -- "1" ShelfRenter

    ShelfRenterRepository "1" o-- "*" ShelfRenter
    MonthlySettlementRepository "1" o-- "*" MonthlySettlement

    IShelfRenterRepository "1" o-- "*" ShelfRenter
    IMonthlySettlementRepository "1" o-- "*" MonthlySettlement

    ShelfRenterViewModel "1" -- "1" IShelfRenterRepository
    ShelfRenterViewModel "1" -- "*" RelayCommand

    MonthlySettlementViewModel "1" -- "1" IMonthlySettlementRepository
    MonthlySettlementViewModel "1" -- "1" IShelfRenterRepository
    MonthlySettlementViewModel "1" -- "*" RelayCommand

    MainViewModel "1" -- "*" RelayCommand

```

  

    
   







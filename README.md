# Reolmarked

# Reolmarked - Design Class Diagrams (DCD)

Dette dokument indeholder projektets to versioner af DCD'et i overensstemmelse med projektkravet: Én model der viser sporbarhed til domænemodellen, og én model der viser eksempler på arkitekturens lag-samspil.

## 1. DCD med sporbarhed til DM (Modellaget)
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
# Reolmarked - Arkitektur DCD (Rental Eksempel)

Dette diagram viser et komplet eksempel på tværs af lagene (Modellag, Repository / Data Access og Præsentation / ViewModel) med fokus på **Rental**-domænet.

```mermaid
classDiagram
    namespace Reolmarked_Core_Models {
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

    namespace Reolmarked_Core_Repositories {
        class IRentalRepository {
            <<interface>>
            + Add(rental: Rental) void
            + Update(rental: Rental) void
            + Delete(shelfNumber: int) void
            + GetAll() List~Rental~
            + GetByShelfNumber(shelfNumber: int) Rental
            + GetByRenterId(renterId: int) List~Rental~
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

    namespace Reolmarked_UI_ViewModels {
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

    IRentalRepository <|.. RentalRepository : implementerer
    RentalViewModel "1" -- "1" IRentalRepository : afhænger af 
    ManageRentalViewModel "1" -- "1" IRentalRepository : afhænger af 
    RentalViewModel "1" o-- "*" Rental : håndterer/opretter
    ManageRentalViewModel "1" o-- "*" Rental : viser/administrerer
    RentalRepository "1" o-- "*" Rental : henter/mapper
```



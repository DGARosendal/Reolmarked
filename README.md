# Reolmarked



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
```mermaid
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
            - _selectedShelf: Shelf?
            - _startDate: DateTime
            + Shelves: ObservableCollection~Shelf~
            + SelectedShelf: Shelf?
            + StartDate: DateTime
            + BookCommand: RelayCommand
            + ConfirmBookingCommand: RelayCommand
            + RentalViewModel(rentalRepository: IRentalRepository, shelfRepository: IShelfRepository, renterRepository: IShelfRenterRepository)
        }

        class ManageRentalViewModel {
            - _rentalRepository: IRentalRepository
            - _selectedRental: Rental?
            + Rentals: ObservableCollection~Rental~
            + SelectedRental: Rental?
            + EndRentalCommand: RelayCommand
            + ManageRentalViewModel(rentalRepository: IRentalRepository, shelfRepository: IShelfRepository, renterRepository: IShelfRenterRepository)
        }
    }

    IRentalRepository <|.. RentalRepository
    RentalViewModel "1" -- "1" IRentalRepository : afhænger af (DI)
    ManageRentalViewModel "1" -- "1" IRentalRepository : afhænger af (DI)
    RentalViewModel "1" o-- "*" Rental : håndterer/opretter
    ManageRentalViewModel "1" o-- "*" Rental : viser/administrerer
    RentalRepository "1" o-- "*" Rental : henter/mapper
```



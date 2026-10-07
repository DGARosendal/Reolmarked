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
        class ShelfRenter {
            + RenterId: int
            + FirstName: string
            + LastName: string
            + PhoneNumber: string
            + Balance: double
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
            + ShelfRenters: ObservableCollection~ShelfRenter~
            + SelectedRenter: ShelfRenter
            + AddCommand: RelayCommand
            + UpdateCommand: RelayCommand
            + ShelfRenterViewModel(renterRepository: IShelfRenterRepository)
        }
    }

    IShelfRenterRepository <|.. ShelfRenterRepository : implementerer
    ShelfRenterViewModel "1" -- "1" IShelfRenterRepository : afhænger af 
    ShelfRenterViewModel "1" o-- "*" ShelfRenter : binder til
    ShelfRenterRepository "1" o-- "*" ShelfRenter : indeholder
```






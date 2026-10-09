using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.Commands;
using System.Globalization;
using System.Windows;
using System.Linq;
using Reolmarked.Core.Models;

namespace Reolmarked.UI.ViewModels
{
    public class MonthlySettlementViewModel : ViewModelBase
    {

        private readonly IMonthlySettlementRepository _monthlySettlementRepository;
        private readonly IShelfRenterRepository _shelfRenterRepository;

        private int _settlementId;
        private int _renterId;
        private int _monthNumber;

        private string _renterName = string.Empty;
        public string RenterName
        {
            get => _renterName;
            set => SetField(ref _renterName, value);
        }

        private string _startDate = string.Empty;

        public string StartDate
        {
            get => _startDate;
            set => SetField(ref _startDate, value);
        }

        private string _endDate;

        public string EndDate
        {
            get => _endDate;
            set => SetField(ref _endDate, value);
        }

        private string _calculationString;

        public string CalculationsString
        {
            get => _calculationString; 
            set => SetField(ref _calculationString, value);
        }

        private double _pricePerShelf;

        public double PricePerShelf
        {
            get => _pricePerShelf;
            set => SetField(ref _pricePerShelf, value);
        }


        public string Month
        {
            get
            {
                if (_monthNumber >= 1 && _monthNumber <= 12)
                {
                    var danishCulture = new CultureInfo("da-DK");
                    string monthName = danishCulture.DateTimeFormat.GetMonthName(_monthNumber);

                    return char.ToUpper(monthName[0], danishCulture) + monthName.Substring(1);
                }
                return string.Empty;
            }
        }
        public int MonthNumber
        {
            get => _monthNumber;
            set
            {
                if (SetField(ref _monthNumber, value))
                {
                    OnPropertyChanged(nameof(Month));
                }
            }
        }

        // private double _totalSales;
        private double _totalSales;

        public double TotalSales
        {
            get => _totalSales;
            set
            {
                if (SetField(ref _totalSales, value))
                {
                    UpdateCalculatedModelValues();
                }
            }
        }

        private int _shelfCount;

        public int ShelfCount
        {
            get => _shelfCount;
            set
            {
                if (SetField(ref _shelfCount, value))
                {
                    UpdateCalculatedModelValues();
                }
            }
        }


        private double _extraDiscount;

        public double ExtraDiscount
        {
            get => _extraDiscount;
            private set
            {
                if (SetField(ref _extraDiscount, value))
                {
                    UpdateCalculatedModelValues();
                }
            }
        }

        #region ============= READ ONLY PROPERTIES HOLDING MODEL CALCULATIONS  =============
        private double _commission;

        public double Commission
        {
            get => _commission;
            set => SetField(ref _commission, value);
        }


        private double _totalShelfRent;

        public double TotalShelfRent
        {
            get => _totalShelfRent;
            set => SetField(ref _totalShelfRent, value);
        }

        private double _finalAmount;

        public double FinalAmount
        {
            get => _finalAmount;
            set => SetField(ref _finalAmount, value);
        }

        private bool _isProcessed;

        public bool IsProcessed
        {
            get => _isProcessed;
            set
            {
                SetField(ref _isProcessed, value);
                ConfirmProcessing.RaiseCanExecuteChanged();
            }
        }
        #endregion

        public RelayCommand ConfirmProcessing { get; }
        public RelayCommand CancelProcessing { get; }


        public MonthlySettlementViewModel(
            IMonthlySettlementRepository monthlySettlementRepository,
            IShelfRenterRepository shelfRenterRepository)
        {
            _monthlySettlementRepository = monthlySettlementRepository;
            _shelfRenterRepository = shelfRenterRepository;

            ConfirmProcessing = new RelayCommand(ConfirmMonthlySettlementProcessing);
            CancelProcessing = new RelayCommand(CancelMonthlySettlementProcessing);
        }

        // Empty Constructor
        public MonthlySettlementViewModel() : this(
            new MonthlySettlementRepository(),
            new ShelfRenterRepository())
        { }


        public void LoadMonthlyRenterSettlement(int renterId)
        {
            // 
            _renterId = renterId;
            var renter = _shelfRenterRepository.GetById(renterId);
           
            if (renter != null)
            {
                RenterName = $"{renter.FirstName} {renter.LastName}";
               
            }

            //
            var existingMonthlySettlment = _monthlySettlementRepository.GetByRenterId(renterId).FirstOrDefault();
            int year = DateTime.Now.Year;

            if (existingMonthlySettlment != null)
            {
                _settlementId = existingMonthlySettlment.SettlementId;
                MonthNumber = existingMonthlySettlment.Month;
                TotalSales = existingMonthlySettlment.TotalSales;
                Commission = existingMonthlySettlment.Commission;
                TotalShelfRent = existingMonthlySettlment.TotalShelfRent;
                ShelfCount = existingMonthlySettlment.ShelfCount;
                ExtraDiscount = existingMonthlySettlment.ExtraDiscount;
                FinalAmount = existingMonthlySettlment.FinalAmount;
                IsProcessed = existingMonthlySettlment.IsProcessed;

                // Calculate start and end date based on saved month and current year context
                if (_monthNumber > DateTime.Now.Month) year--;
                DateTime start = new DateTime(year, _monthNumber, 1);
                DateTime end = new DateTime(
                    year, _monthNumber,
                    DateTime.DaysInMonth(year, _monthNumber));

                StartDate = start.ToString("dd-MM-yyyy");
                EndDate = end.ToString("dd-MM-yyyy");
                CalculationsString = $"({TotalSales}-{Commission}-{TotalShelfRent}-{ExtraDiscount}) =";
                PricePerShelf = TotalShelfRent / ShelfCount;
            }
            else
            {
                // Properly initialize prior month logic
                var priorMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                MonthNumber = priorMonthDate.Month;
                year = priorMonthDate.Year;

                ShelfCount = 1;
                IsProcessed = false;

                // Triggers initial calculation via logic in the model
                UpdateCalculatedModelValues();
            }
        }

        /// <summary>
        /// Delegates all calculation logic to a temporary instance of the MonthlySettlement Model.
        /// </summary>
        private void UpdateCalculatedModelValues()
        {
            try
            {
                var temporaryMonthlySettlement = new MonthlySettlement(
                    renterId: _renterId,
                    month: _monthNumber,
                    totalSales: _totalSales,
                    shelfCount: _shelfCount,
                    extraDiscount: _extraDiscount,
                    isProcessed: _isProcessed
                    );

                Commission = temporaryMonthlySettlement.Commission;
                TotalShelfRent = temporaryMonthlySettlement.TotalShelfRent;
                FinalAmount = temporaryMonthlySettlement.FinalAmount;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Calculation error: {ex.Message}");
            }
        }

        private void ConfirmMonthlySettlementProcessing()
        {
            var temporaryMonthlySettlement = new MonthlySettlement(
                settlementId: _settlementId,
                renterId: _renterId,
                month: _monthNumber,
                totalSales: _totalSales,
                commission: _commission,
                totalShelfRent: _totalShelfRent,
                shelfCount: _shelfCount,
                extraDiscount: _extraDiscount,
                finalAmount: _finalAmount,
                isProcessed: _isProcessed
                );

            // Save updated status
            if (_settlementId > 0)
            {
                _monthlySettlementRepository.Update(temporaryMonthlySettlement);
            }

            // Close window
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        private void CancelMonthlySettlementProcessing()
        {
            // Does nothing, closes window
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }   
}

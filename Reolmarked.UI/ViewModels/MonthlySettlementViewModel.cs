using Microsoft.VisualBasic;
using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.Commands;
using Reolmarked.UI.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;

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

        private string _startDate;

        public string StartDate
        {
            get { return _startDate; }
            set => SetField(ref _startDate, value);
        }

        private string _endDate;

        public string EndDate
        {
            get { return _endDate; }
            set => SetField(ref _endDate, value);
        }

        // private int _month;
        private int _month;

        public string Month
        {
            get { 
                if(_monthNumber >= 1 && _monthNumber <= 12)
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
                    OnPropertyChanged(nameof(Month)); // Tells WPF the Danish month name needs to re-render
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
                    CalculateTotals();
                }
            }
        }

        // private double _commission;
        private double _comission;

        public double Comission
        {
            get => _comission;
            set => SetField(ref _comission, value);
        }

        // private int _shelfCount;
        private int _shelfCount;

        public int ShelfCount
        {
            get => _shelfCount;
            set
            {
                if (SetField(ref _shelfCount, value))
                {
                    CalculateTotals();
                }
            }
        }

        private double _pricePerShelf;

        public double PricePerShelf
        {
            get { return _pricePerShelf; }
            set
            {
                if (SetField(ref _pricePerShelf, value))
                {
                    CalculateTotals();
                }
            }

        }

        // private double _totalShelfRent;
        private double _totalShelfRent;


        public double TotalShelfRent
        {
            get => _totalShelfRent;
            set => SetField(ref _totalShelfRent, value);
        }



        // private double _extraDiscount;
        private double _extraDiscount;

        public double ExtraDiscount
        {
            get => _extraDiscount;
            set
            {
                if (SetField(ref _extraDiscount, value))
                {
                    CalculateTotals();
                }
            }
        }

        // private double _finalAmount;
        private double _finalAmount;

        public double FinalAmount
        {
            get => _finalAmount;
            set => SetField(ref _finalAmount, value);
        }

        // private bool _isProcessed;
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

        public RelayCommand ConfirmProcessing { get;  }
        public RelayCommand CancelProcessing { get; }


        public MonthlySettlementViewModel(IMonthlySettlementRepository monthlySettlementRepository, IShelfRenterRepository shelfRenterRepository )
        {
            _monthlySettlementRepository = monthlySettlementRepository;
            _shelfRenterRepository = shelfRenterRepository;

            ConfirmProcessing = new RelayCommand(ConfirmMonthlySettlementProcessing);
            CancelProcessing = new RelayCommand(CancelMonthlySettlementProcessing);
        }

        // Empty 
        public MonthlySettlementViewModel() : this(new MonthlySettlementRepository(), new ShelfRenterRepository()) { }


        public void LoadMonthlyRenterSettlement(int renterId)
        {  // 
            _renterId = renterId;
            var renter = _shelfRenterRepository.GetById(renterId);
            if (renter != null)
            {
                RenterName = $"{renter.FirstName} {renter.LastName}";
            }

            //
            var existingMonthlySettlment = _monthlySettlementRepository.GetByRenterId(renterId).FirstOrDefault();
            int year = DateTime.Now.Year;
            
            if(existingMonthlySettlment != null)
            {
                _settlementId = existingMonthlySettlment.SettlementId;
                MonthNumber = existingMonthlySettlment.Month;
                TotalSales = existingMonthlySettlment.TotalSales;
                Comission = existingMonthlySettlment.Commission;
                TotalShelfRent = existingMonthlySettlment.TotalShelfRent;
                ShelfCount = existingMonthlySettlment.ShelfCount;
                ExtraDiscount = existingMonthlySettlment.ExtraDiscount;
                FinalAmount = existingMonthlySettlment.FinalAmount;
                IsProcessed = existingMonthlySettlment.IsProcessed;

                // Calculate start and end date based on saved month and current year context
                year = DateTime.Now.Year;
                // Handles transition to prior year if we're in January and last month was fx. December.
                if (_monthNumber > DateTime.Now.Month) year--;
                
                DateTime start = new DateTime(year, _monthNumber, 1);
                // Calculates the last date of the month based on year and month.
                DateTime end = new DateTime(year, _monthNumber, DateTime.DaysInMonth(year, _monthNumber));

                StartDate = start.ToString("dd-MM-yyyy");
                EndDate = end.ToString("dd-MM-yyyy");
            }
            else
            {
                // Properly initialize prior month logic
                var priorMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                _monthNumber = priorMonthDate.Month;
                year = priorMonthDate.Year;

                ShelfCount = 1;
                IsProcessed = false;
            }
        }
        


        private void CalculateTotals()
        {
            // Comission is 10% of TotalSales
            Comission = _totalSales * 0.10;

            // Price Per Shelf is based on numbers of shelves divided into price tiers 
            PricePerShelf = _shelfCount switch
            {
                1 => 850.0,
                2 or 3 => 825.0,
                >= 4 => 800.0,
                _ => 0.0
            };
            
            // TotalShelfRent is based on numbers of shelves times Price Per Shelf
            TotalShelfRent = _shelfCount * _pricePerShelf;

            // Calculates Final Amount (Total Sales - Comission + Total Shelf Rent - (Extra Discount on Shelves)
            FinalAmount = (_totalSales - _comission + _totalShelfRent);
            // WARNING: TODO Haven't included extra discount for now
        }

        private void ConfirmMonthlySettlementProcessing()
        {
            IsProcessed = true;
        }

        private void CancelMonthlySettlementProcessing()
        {
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

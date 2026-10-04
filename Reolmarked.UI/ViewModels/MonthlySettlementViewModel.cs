using Reolmarked.Core.Interfaces;
using Reolmarked.UI.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reolmarked.UI.ViewModels
{
    public class MontlySettlementViewModel : ViewModelBase
    {
        // private int _settlementId;
        //private int _renterId;
        private readonly IMonthlySettlementRepository _monthlySettlementRepository;

        private readonly IShelfRenterRepository shelfRenterRepository;

        // private int _month;
        private int _month;

        public int Month
        {
            get { return _month; }
            set { _month = value; }
        }

        // private double _totalSales;
        private double _totalSales;

        public double TotalSales
        {
            get { return _totalSales; }
            set { _totalSales = value; }
        }

        // private double _commission;
        private double _comission;

        public double Comission
        {
            get { return _comission; }
            set { _comission = value; }
        }

        // private double _totalShelfRent;
        private double _totalShelfRent;

        public double TotalShelfRent
        {
            get { return _totalShelfRent; }
            set { _totalShelfRent = value; }
        }

        // private int _shelfCount;
        private int _shelfCount;

        public int ShelfCount
        {
            get { return _shelfCount; }
            set { _shelfCount = value; }
        }

        // private double _extraDiscount;
        private double _extraDiscount;

        public double ExtraDiscount
        {
            get { return _extraDiscount; }
            set { _extraDiscount = value; }
        }

        // private double _finalAmount;
        private double _finalAmount;

        public double FinalAmount
        {
            get { return _finalAmount; }
            set { _finalAmount = value; }
        }

        // private bool _isProcessed;
        private bool _isProcessed;

        public bool IsProcessed
        {
            get { return _isProcessed; }
            set { _isProcessed = value; }
        }

        public RelayCommand ConfirmProcessing { get;  }

        public void LoadMonthlySettlement()
        {
            // TODO: 
        }
        

        private bool ConfirmMonthlySettlementProcessing(object? parameter)
        {
            // TODO: 

            return true;
        }
    }
}

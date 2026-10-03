using System;

namespace Reolmarked.Core.Models
{
    public class MonthlySettlement
    {
        // Private backing fields matching the UML class diagram
        private int _settlementId;
        private int _renterId;
        private int _month;
        private double _totalSales;
        private double _commission;
        private double _totalShelfRent;
        private int _shelfCount;
        private double _extraDiscount;
        private double _finalAmount;
        private bool _isProcessed;

        // Properties with encapsulation
        public int SettlementId
        {
            get => _settlementId;
            set => _settlementId = value;
        }

        public int RenterId
        {
            get => _renterId;
            set => _renterId = value;
        }

        public int Month
        {
            get => _month;
            set
            {
                if (value < 1 || value > 12)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Måned skal være et tal mellem 1 og 12.");
                }
                _month = value;
            }
        }

        public double TotalSales
        {
            get => _totalSales;
            set => _totalSales = value;
        }

        public double Commission
        {
            get => _commission;
            set => _commission = value;
        }

        public double TotalShelfRent
        {
            get => _totalShelfRent;
            set => _totalShelfRent = value;
        }

        public int ShelfCount
        {
            get => _shelfCount;
            set => _shelfCount = value;
        }

        public double ExtraDiscount
        {
            get => _extraDiscount;
            set => _extraDiscount = value;
        }

        public double FinalAmount
        {
            get => _finalAmount;
            set => _finalAmount = value;
        }

        public bool IsProcessed
        {
            get => _isProcessed;
            set => _isProcessed = value;
        }

        // Full constructor matching the UML signature
        public MonthlySettlement(
            int settlementId,
            int renterId,
            int month,
            double totalSales,
            double commission,
            double totalShelfRent,
            int shelfCount,
            double extraDiscount,
            double finalAmount,
            bool isProcessed)
        {
            SettlementId = settlementId;
            RenterId = renterId;
            Month = month;
            TotalSales = totalSales;
            Commission = commission;
            TotalShelfRent = totalShelfRent;
            ShelfCount = shelfCount;
            ExtraDiscount = extraDiscount;
            FinalAmount = finalAmount;
            IsProcessed = isProcessed;
        }

        // Overloaded constructor for creating settlements before SQL Server generates SettlementId
        public MonthlySettlement(
            int renterId,
            int month,
            double totalSales,
            double commission,
            double totalShelfRent,
            int shelfCount,
            double extraDiscount,
            double finalAmount,
            bool isProcessed)
            : this(0, renterId, month, totalSales, commission, totalShelfRent, shelfCount, extraDiscount, finalAmount, isProcessed) { }
    }
}
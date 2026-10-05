// SRP: This class is responsible for managing monthly settlements for renters. It makes sure
// that properties in relation to this business logic is encapsulated and validated.

using System;

namespace Reolmarked.Core.Models
{
    public class MonthlySettlement
    {
        // !! COMMISSION PERCENT !!
        // BR2 from UC-4: The marketplace always takes a fixed 10% commission on total sales.
        // Stored as a percentage (10 = 10%). If the fixed commission rate ever changes,
        // this is the single place to update it.
        private const double COMMISSION_PERCENT = 10;

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

        // Month must be a valid calendar month (1-12), matching UC-4.
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

        // Total sales amount for the renter in the settlement period.
        // A renter may have sold nothing (0 DKK), but a negative amount makes no sense.
        public double TotalSales
        {
            get => _totalSales;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Samlet salg kan ikke være negativt.");
                }
                _totalSales = value;
            }
        }

        // The three values below are calculated once when a new settlement is created,
        // and then saved. They have a private setter, so they can be read from the outside
        // but not changed. That way an old settlement keeps the numbers it was created with,
        // even if the rates change later.

        // BR2 from UC-4: Commission is 10% of total sales.
        public double Commission
        {
            get => _commission;
            private set => _commission = value;
        }

        // BR1 from UC-4: Volume discount on shelf rent.
        public double TotalShelfRent
        {
            get => _totalShelfRent;
            private set => _totalShelfRent = value;
        }

        // BR3 from UC-4: Final amount. Can be positive (payout to renter) or negative
        // (renter owes money, see Exception flow 1A in UC-4).
        public double FinalAmount
        {
            get => _finalAmount;
            private set => _finalAmount = value;
        }

        // Number of shelves the renter is paying rent for in the settlement period.
        // 0 is allowed (e.g. if the renter has no active rentals in the period, according
        // to UC-4 Domain model (ShelfRenter can have 0..* Rentals)).
        public int ShelfCount
        {
            get => _shelfCount;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Antal reoler kan ikke være negativt.");
                }
                _shelfCount = value;
            }
        }

        // The marketplace can offer an extra discount to the renter in special cases.
        // A discount would not make sense as a negative number.
        public double ExtraDiscount
        {
            get => _extraDiscount;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Ekstra rabat kan ikke være negativ.");
                }
                _extraDiscount = value;
            }
        }

        public bool IsProcessed
        {
            get => _isProcessed;
            set => _isProcessed = value;
        }

        /// <summary>
        /// !! SHELF RENT CALCULATION !!
        /// If the rates ever change, then this is the single place to update them.
        /// BR1 from UC4: Volume discount on shelf rent.
        /// 1 shelf: 850 DKK/month
        /// 2-3 shelves: 825 DKK/month/shelf
        /// 4+ shelves: 800 DKK/month/shelf
        /// The method returns a double because we also use it to calculate the total
        /// shelf rent, which, in turn, is used to calculate the final amount that
        /// already relies on double precision.
        /// </summary>
        private static double CalculateShelfRent(int shelfCount)
        {
            if (shelfCount == 0)
                return 0;

            if (shelfCount == 1)
                return 850;

            if (shelfCount <= 3)
                return 825 * shelfCount;

            return 800 * shelfCount;
        }

        // This constructor is used when a brand new settlement is made.
        // It calculates Commission, TotalShelfRent and FinalAmount once and stores them.
        public MonthlySettlement(
            int renterId,
            int month,
            double totalSales,
            int shelfCount,
            double extraDiscount,
            bool isProcessed = false)
        {
            RenterId = renterId;
            Month = month;
            TotalSales = totalSales;
            ShelfCount = shelfCount;
            ExtraDiscount = extraDiscount;
            IsProcessed = isProcessed;

            Commission = TotalSales * (COMMISSION_PERCENT / 100);
            TotalShelfRent = CalculateShelfRent(ShelfCount);
            FinalAmount = TotalSales - Commission - TotalShelfRent + ExtraDiscount;
        }

        // This constructor is used when reading an old settlement back from the database.
        // It uses the saved numbers as they are, so old settlements do not change
        // just because the rates have been updated since.
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
    }
}
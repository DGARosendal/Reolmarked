namespace Reolmarked.Core.Models
{
    public class Rental
    {
        private int _rentalId;
        private int _shelfNumber;
        private DateTime _startDate;
        private DateTime? _endDate;
        private int _renterId;

        public int RentalId { get => _rentalId; set => _rentalId = value; }
        public int ShelfNumber { get => _shelfNumber; set => _shelfNumber = value; }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (value == DateTime.MinValue)
                    throw new ArgumentException("Startdato er ikke angivet korrekt.");

                if (value.Day != 1)
                    throw new ArgumentException("En udlejning skal starte på den 1. i en måned.");

                // If the rental already has an EndDate, the new StartDate must not be after it.
                // EndDate checks the same thing the other way around, so both setters agree.
                if (_endDate.HasValue && value > _endDate.Value)
                    throw new ArgumentException("Startdato kan ikke være efter slutdato.");

                _startDate = value;
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (value.HasValue && value.Value < _startDate)
                    throw new ArgumentException("Slutdato kan ikke være før startdato.");

                _endDate = value;
            }
        }

        public int RenterId { get => _renterId; set => _renterId = value; }

        // Main constructor matching MapRental in RentalRepository
        public Rental(int rentalId, int shelfNumber, DateTime startDate, DateTime? endDate, int renterId)
        {
            RentalId = rentalId;
            ShelfNumber = shelfNumber;
            StartDate = startDate;
            EndDate = endDate;
            RenterId = renterId;
        }

        // Convenience constructor for creating new rentals in ViewModels
        public Rental(int shelfNumber, DateTime startDate, int renterId, DateTime? endDate = null)
            : this(0, shelfNumber, startDate, endDate, renterId) { }

        public void SetTerminationDate()
        {
            DateTime now = DateTime.Now;
            DateTime referenceDate = StartDate > now ? StartDate : now;
            // Guyyaa jalqaba ji'a itti aanuu argachuudhaan guyyaa 1 irraa hir'isa
            DateTime startOfNextMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1).AddMonths(1);
            EndDate = startOfNextMonth.AddDays(-1);
        }

        public bool isTerminated()
        {
            return EndDate.HasValue;
        }
    }

}
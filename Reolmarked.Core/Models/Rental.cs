// SRP: This class is responsible for the integrity of a Rental.
// It holds the rental's data in its fields and only accepts values
// that pass validation.

using System;

namespace Reolmarked.Core.Models
{
    public class Rental
    {
        // Backing fields. Private so they can only be changed through the
        // properties below. That way every value passes validation first.
        private int _shelfNumber;
        private DateTime _startDate;
        private int _renterId;

        // Represents the shelf (reol) being rented. This is a positive
        // integer and matches the ShelfNumber of an existing Shelf (already gets 
        // validated through Shelf.cs).
        public int ShelfNumber
        {
            get { return _shelfNumber; }
            set { _shelfNumber = value; }
        }

        // Represents the date the rental begins. 
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                // The ViewModel defaults StartDate to today, but we can't rely on that
                // always being the case if the UI is ever replaced. So we keep a backup
                // validation here.
                //
                // DateTime.MinValue (01-01-0001) is what the field holds if it is never
                // set. Since that is not a valid start date for a rental, we use it as a
                // signal that the value was never assigned, and gently remind the user.
                if (value == DateTime.MinValue)
                {
                    throw new ArgumentException("Startdato er ikke angivet korrekt. Vælg venligst en gyldig dato.");
                }

                // Business rule validation: a booking may only start on the 1st of a month.
                // Without this, the user could book from e.g. the 14th, which conflicts with
                // the agreement about monthly billing.
                if (value.Day != 1)
                {
                    throw new ArgumentException("A booking must start on the 1st of a month.");
                }

                _startDate = value;
            }
        }

        // Represents the renter (reollejer). Matches the RenterId of an
        // existing ShelfRenter.
        public int RenterId
        {
            get { return _renterId; }
            set { _renterId = value; }
        }

        // When we create a new Rental, all three values must be given as
        // parameters in the constructor.
        public Rental(int shelfNumber, DateTime startDate, int renterId)
        {
            ShelfNumber = shelfNumber;
            StartDate = startDate;
            RenterId = renterId;
        }
    }
}
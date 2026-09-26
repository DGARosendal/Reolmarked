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
        private int _userId;

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
                // DateTime.MinValue (01-01-0001) is what the field holds if
                // it is never set. That is not a valid start date for a rental.
                // Therefore, if the value has not been assigned by the user, it will be 01-01-0001. 
                // We check for this value as a way of determining whether the value has not been set, 
                // and gently remind the user to set the date if this is the case. 
                if (value == DateTime.MinValue)
                {
                    throw new ArgumentException("Startdato er ikke angivet. Vælg venligst en gyldig dato.");
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

        // Represents the employee who created the rental. Useful for
        // auditing: "who booked this shelf?".
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        // When we create a new Rental, all four values must be given as
        // parameters in the constructor.
        public Rental(int shelfNumber, DateTime startDate, int renterId, int userId)
        {
            ShelfNumber = shelfNumber;
            StartDate = startDate;
            RenterId = renterId;
            UserId = userId;
        }
    }
}
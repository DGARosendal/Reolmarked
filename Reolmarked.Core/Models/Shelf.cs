using System.Net.NetworkInformation;

namespace Reolmarked.Core.Models
{


    public class Shelf
    {
        // These fields hold the real values. The properties below check the value (validate) before saving it.
        private int _shelfNumber;
        private Configuration _configuration;
        private Status _status;


        // Represents the number of the shelf (reol). This is a positive integer, and 0 is not used.
        public int ShelfNumber
        {
            get { return _shelfNumber; }
            set
            {
                // There are no negative shelf numbers, and 0 is not used either
                // We could, theoretically, limit the shelf number to 80 here given the case requirements, but
                // that would make the code less flexible (if, say, Reolmarked ever wanted to expand beyond 80 shelves)
                // without really adding much benefit to them. 
                if (value <= 0)
                {
                    // If a shelf number is invalid, we throw an error explaining to the user what the problem is.
                    throw new ArgumentException("Reolnummer skal være større end 0.");
                }

                // If the shelf number is valid, we save it.
                _shelfNumber = value;
            }
        }

        // Represents the configuration of the shelf (reol). We have an enum for this, currently
        // set up with either 3 shelves and a hanger, or 6 shelves, as options.
        public Configuration Configuration
        {
            get { return _configuration; }
            set
            {
                // Enum.IsDefined checks that the value is one of the options in the enum.
                // If it is not, we throw an error to the user, gently letting them know what the problem is.
                if (!Enum.IsDefined(typeof(Configuration), value))
                {
                    throw new ArgumentException("Ugyldig reolkonfiguration.");
                }

                _configuration = value;
            }
        }

        public Status Status
        {
            get { return _status; }
            set
            {
                // Same check as for Configuration above.
                if (!Enum.IsDefined(typeof(Status), value))
                {
                    throw new ArgumentException("Ugyldig reolstatus.");
                }

                _status = value;
            }
        }

        

        /// <summary>
        /// Setter for field _status.
        /// Throws exception if change in Status is invalid.
        /// </summary>
        /// <param name="oldStatus"></param>
        /// <param name="newStatus"></param>
        public void UpdateStatus(Status oldStatus, Status newStatus)
        {
            // Same check as for Configuration above.
            if (!Enum.IsDefined(typeof(Status), newStatus))
            {
                throw new ArgumentException("Ugyldig reolstatus.");
            }
            // Can't change status of a booked or terminated Shelf
            else if (oldStatus != newStatus && (oldStatus == Status.Booket || oldStatus == Status.Opsagt))
            {
                string status = (oldStatus == Status.Booket) ? "booket" : "opsagt";
                throw new ArgumentException($"Status kan ikke ændres på en {status} reol");
            }
            // Can't set status of Shelf to booked
            else if (oldStatus != newStatus && (newStatus == Status.Booket || newStatus == Status.Opsagt))
            {
                string status = (newStatus == Status.Booket) ? "booket" : "opsagt";
                throw new ArgumentException($"Status kan ikke sættes til {status}.");
            }
            Status = newStatus;
        }

        public Shelf(int shelfNumber, Configuration configuration, Status status)
        {
            // These use the properties above, so we know that all three values are validated.
            // If one is invalid, an error is thrown above and the Shelf is never created.
            ShelfNumber = shelfNumber;
            Configuration = configuration;
            Status = status;
        }

        /// <summary>
        /// Calls Shelf contructor with validation for newly created Shelves
        /// </summary>
        /// <param name="shelfNumber"></param>
        /// <param name="configuration"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Shelf CreateNewShelf(int shelfNumber, Configuration configuration, Status status)
        {
            // Can't add new Shelf with booked or terminated Status
            if (status == Status.Booket || status == Status.Opsagt)
            {
                string statusString = (status == Status.Booket) ? "booket" : "opsagt";
                throw new ArgumentException($"Status kan ikke sættes til {statusString} på en ny reol.");
            }

            return new Shelf(shelfNumber, configuration, status);
        }

        /// <summary>
        /// Check if Shelf is valid for deletion
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool CanBeDeleted()
        {
            // Can't delete shelf that is booked or terminated
            if (Status == Status.Booket ||  Status == Status.Opsagt)
            {
                string status = (Status == Status.Booket) ? "booket" : "opsagt";
                throw new InvalidOperationException($"Man kan ikke slette en {status} reol.");
            }

            return true;
        }
    }
}
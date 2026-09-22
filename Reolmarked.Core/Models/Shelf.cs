using System.Net.NetworkInformation;

namespace Reolmarked.Core.Models
{

    
    // The two ways a shelf (currently) can be set up:
    public enum Configuration
    {
        TreHylderOgBøjle,   // 3 shelves and a hanger
        SeksHylder          // 6 shelves
    }

    public enum Status
    {
        Ledig,       // Free, can be rented
        Booket,      // Rented by a renter
        Opsagt,      // Renter has terminated, but the shelf is not free yet
        UdeAfDrift   // Temporarily out of service
    }

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

        public Shelf(int shelfNumber, Configuration configuration, Status status)
        {
            // These use the properties above, so we know that all three values are validated.
            // If one is invalid, an error is thrown above and the Shelf is never created.
            ShelfNumber = shelfNumber;
            Configuration = configuration;
            Status = status;
        }
    }
}
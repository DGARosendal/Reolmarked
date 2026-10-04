// SRP: This class is responsible for the integrity of a ShelfRenter.
// It holds the renter's info in its fields and only accepts values that
// pass validation.

using System;

namespace Reolmarked.Core.Models
{
    public class ShelfRenter
    {

        // !! MAINTENANCE NOTE ON SINGLE SOURCE OF TRUTH AND DATABASE !!
        // !! CHANGE THE BELOW CONSTANTS TO MATCH THE DATABASE !!

        // In the database, the FirstName and LastName columns are both NVarChar(50).
        // The reason why we also have a validation check here based on this number is
        // so that we can catch any errors regarding the length of a name here first, in the model-layer,
        // before it reaches the database, and then present the user with a gentle Danish error message, 
        // instructing them as to what the error is and guiding them back on their happy path.
        // In short, by catching length-errors in the model-layer here, we can make the program more user friendly.  

        // The downside to this is that we lose the "single source of truth", 
        // as 50 (as well as MaxPhoneNumberLength = 20, which has NVarChar(20) in the DB)
        // now also are values stored here in the model-layer. As such, we risk sync issues between
        // these two values. If the length of a name changes to, say, 65 characters in the database, we
        // have to remember to change it to 65 here too!

        // The way we have chosen to handle this is to be explicit in the comments about it, setting up 
        // a clear maintenance note for future developers, so that they are aware of the trade-off,
        // what we gain from it, and what we risk from it. 

        // Same size as the FirstName and LastName columns in the database.
        private const int MaxNameLength = 50;

        // Same size as the PhoneNumber column in the database.
        private const int MaxPhoneNumberLength = 20;


        // Backing fields. Private so they can only be changed through the
        // properties below. That way every value passes validation first.
        private string _firstName;
        private string _lastName;
        private string _phoneNumber;

        private double _balance;

        public int RenterId { get; set; }

        public string FirstName
        {
            // Returns the stored value.
            get
            {
                return _firstName;
            }

            // Validation is done in ValidateName, which either throws an error or returns the 
            // validated and trimmed string, which we store directly in the backing field, _firstName.
            set
            {
                _firstName = ValidateName(value, "Fornavn");
            }
        }

        public string LastName
        {
            get
            {
                return _lastName;
            }

            // Same as FirstName above. 
            set
            {
                _lastName = ValidateName(value, "Efternavn");
            }
        }


        public string PhoneNumber
        {
            get
            {
                return _phoneNumber;
            }

            // ValidatePhoneNumber returns the validated and cleaned phone number, which
            // we store directly in the backing field, _phoneNumber.
            set
            {
                _phoneNumber = ValidatePhoneNumber(value);
            }
        }

        public double Balance
        {
            get
            {
                return _balance;
            }

            set
            {
                _balance = value;
            }
        }

        // When we create a new ShelfRenter, all three (validated) values must
        // be given as parameters in the constructor.
        public ShelfRenter(int RenterId, string firstName, string lastName, string phoneNumber, double balance = 0)
        {
            this.RenterId = RenterId;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Balance = balance;
        }




        // Used by both FirstName and LastName since the rule is the same.
        // "fieldName" is just the word we show in the error message (either "Fornavn" or "Efternavn").
        // Returns the trimmed and validated name as a string, which we utilize in our setters in our properties above.
        public static string ValidateName(string name, string fieldName)
        {
            // Catches null, empty text, and text with only spaces.
            if (string.IsNullOrWhiteSpace(name))
            {
                // Gentle message pointing to that either the first name or last name is
                // empty, and that it must be filled in.
                throw new ArgumentException(fieldName + " må ikke være tomt.");
            }

            

            // Removes spaces at the start/end before we count the length.
            // This way we don't have to tell the user that "  Anne-Marie  " is too
            // long, when it's really just "Anne-Marie".
            // An added benefit here is also that we avoid storing names with leading/trailing
            // spaces in the database, which would be a waste of space.
            string trimmed = name.Trim();

            // The trimmed name can't be longer than the database column allows. This is set
            // above via a constant, as well as in the database.
            if (trimmed.Length > MaxNameLength)
            {
                throw new ArgumentException(fieldName + " må højst være " + MaxNameLength + " tegn.");
            }

            // We loop through each letter and only allow letters, spaces,
            // dashes and apostrophes in a name. Numbers and symbols like @ # ! are not OK.
            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];

                // If the specific character, c, is a letter (works with A-Z, Æ, Ø, Å and letters with accents), then we continue.
                if (char.IsLetter(c))
                {
                    continue;
                }

                // We allow spaces, dashes and apostrophes so names like
                // "Anne-Marie", "Van Der Berg" and "O'Connor" still work.
                if (c == ' ' || c == '-' || c == '\'')
                {
                    continue;
                }

                // Anything else means the name is not valid and we throw a gentle error message to the
                // user, guiding them back on the happy path.
                throw new ArgumentException(
                    fieldName + " må kun indeholde bogstaver, mellemrum, bindestreg og apostrof.");
            }

            // Returns the trimmed and validated name so the setter can store it in the backing field above.
            return trimmed;
        }

        // Validates the phone number and returns this to the setter in the
        // PhoneNumber property above.
        private static string ValidatePhoneNumber(string phoneNumber)
        {
            // Empty or whitespace-only is not allowed in a phone number.
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Telefonnummer må ikke være tomt.");
            }

            // We trim the number to remove leading/trailing spaces before we count the
            // length, like we did with names above.
            string trimmed = phoneNumber.Trim();

            // Can't be longer than the database column allows. This is set
            // above via a constant, as well as in the database.
            if (trimmed.Length > MaxPhoneNumberLength)
            {
                throw new ArgumentException("Telefonnummer må højst være " + MaxPhoneNumberLength + " tegn.");
            }

            // Remember if we have seen at least one digit.
            // This stops inputs like "+" or "---" that are only formatting.
            bool hasDigit = false;

            // Look at one character at a time.
            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];

                // If the char is a number, we set hasDigit to true and we continue. 
                if (char.IsDigit(c))
                {
                    hasDigit = true;
                    continue;
                }

                // Space and dash are formatting that might be present in the number, which is fine and we continue.
                if (c == ' ' || c == '-')
                {
                    continue;
                }

                // The char can also be a '+' sign, which is used
                // before country codes in phone numbers (e.g. +45 12345678).
                if (c == '+')
                {
                    // As such, '+' is only allowed as the very first character and NOT anywhere else in the number.
                    if (i != 0)
                    {
                        throw new ArgumentException("Telefonnummer må kun have + forrest.");
                    }

                    // '+' must be followed by a digit, otherwise it's meaningless, so we check whether the 
                    // length of the number is 1 (only contains +) OR if the next character in the string IS NOT a digit.
                    if (trimmed.Length == 1 || !char.IsDigit(trimmed[1]))
                    {
                        // If the string only contains + or what follows + isn't a number, then we throw 
                        // an error.
                        throw new ArgumentException("+ skal efterfølges af et tal.");
                    }

                    continue;
                }

                // Anything else (letters, @, #, etc.) is not allowed.
                throw new ArgumentException("Telefonnummer må kun indeholde tal, mellemrum, bindestreg og + forrest.");
            }

            // If we never saw a digit in the string (hasDigit=false), the number is not real (e.g. "+", "---", " + ").
            if (!hasDigit)
            {
                throw new ArgumentException("Telefonnummer skal indeholde mindst ét tal.");
            }

            // Returns the validated phonenumber so the PhoneNumber-property can store it via its setter above.
            // We also replace any remaining redundant characters in the number before handing it 
            // back. This means that we don't store needless characters in our database ("+45 12 34 56 78"
            // works equally well compared to "+4512345678" but takes up less space). 
            return trimmed.Replace(" ", "").Replace("-", "");
        }

    public ShelfRenter(string firstName, string lastName, string phoneNumber, double balance = 0)
            : this(0, firstName, lastName, phoneNumber, balance) { }
    }
}
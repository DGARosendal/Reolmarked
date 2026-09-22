using System;

namespace Reolmarked.Core.Models
{
    public class ShelfRenter
    {
        private string _firstName;
        private string _lastName;
        private string _phoneNumber;

        public int RenterId { get; set; }

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("First name cannot be empty.");
                _firstName = value;
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Last name cannot be empty.");
                _lastName = value;
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Phone number cannot be empty.");
                _phoneNumber = value;
            }
        }

        public ShelfRenter() { }

        public ShelfRenter(int renterId, string firstName, string lastName, string phoneNumber)
        {
            RenterId = renterId;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
        }

        public ShelfRenter(string firstName, string lastName, string phoneNumber)
            : this(0, firstName, lastName, phoneNumber) { }
    }
}
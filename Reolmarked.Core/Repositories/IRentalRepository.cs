// SRP: This interface defines the contract for Rental data access.
// It requires implementing six methods covering CRUD-functionality. 

using System.Collections.Generic;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Repositories
{
    // Contract for Rental data access.
    public interface IRentalRepository
    {
        // Create
        void Add(Rental rental);

        // Update (e.g., if a start date needs correcting)
        void Update(Rental rental);

        // Delete (e.g., if a renter cancels)
        void Delete(int shelfNumber);

        // Read all rentals
        List<Rental> GetAll();

        // Read a single rental by shelf number (PK)
        Rental GetByShelfNumber(int shelfNumber);

        // Read all rentals for one renter
        List<Rental> GetByRenterId(int renterId);
    }
}
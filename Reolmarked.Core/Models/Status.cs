// SRP: Enum that defines the possible statuses of a shelf.

namespace Reolmarked.Model
{
    // The four statuses a shelf can have:
    public enum Status
    {
        Ledig,       // Free, can be rented
        Booket,      // Rented by a renter
        Opsagt,      // Renter has terminated, but the shelf is not free yet
        UdeAfDrift   // Temporarily out of service
    }
}
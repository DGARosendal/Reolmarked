using System;
using System.Collections.Generic;
using System.Text;

namespace Reolmarked.Core.Models
{
    public enum Status
    {
        Ledig,       // Free, can be rented
        Booket,      // Rented by a renter
        Opsagt,      // Renter has terminated, but the shelf is not free yet
        UdeAfDrift   // Temporarily out of service
    }
}

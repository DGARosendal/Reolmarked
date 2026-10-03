using System.Collections.Generic;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Interfaces
{
    public interface IShelfRenterRepository
    {
        void Add(ShelfRenter renter);
        void Update(ShelfRenter renter);
        void Delete(int renterId);
        List<ShelfRenter> GetAll();
        ShelfRenter GetById(int renterId);
    }
}
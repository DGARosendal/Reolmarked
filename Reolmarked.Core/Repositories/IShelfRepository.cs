using System.Collections.Generic;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Repositories
{
    public interface IShelfRepository
    {
        void Add(Shelf shelf);
        void Update(Shelf shelf);
        void Delete(int shelfNumber);
        List<Shelf> GetAll();
        Shelf GetById(int shelfNumber);
    }
}
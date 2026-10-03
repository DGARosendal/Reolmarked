using System.Collections.Generic;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Interfaces
{
    public interface IMonthlySettlementRepository
    {
        void Add(MonthlySettlement settlement);
        void Update(MonthlySettlement settlement);
        List<MonthlySettlement> GetAll();
        MonthlySettlement? GetById(int settlementId);
        List<MonthlySettlement> GetByRenterId(int renterId);
    }
}
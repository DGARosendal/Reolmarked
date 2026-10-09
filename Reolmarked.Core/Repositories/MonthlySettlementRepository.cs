using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Repositories
{
    public class MonthlySettlementRepository : IMonthlySettlementRepository
    {
        private readonly string _connectionString;

        public MonthlySettlementRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Fetches all settlement rows directly via ADO.NET
        public List<MonthlySettlement> GetAll()
        {
            List<MonthlySettlement> settlements = new List<MonthlySettlement>();
            string sql = @"SELECT SettlementId, RenterId, Month, TotalSales, Commission, 
                                  TotalShelfRent, ShelfCount, ExtraDiscount, FinalAmount, IsProcessed 
                           FROM dbo.MONTHLY_SETTLEMENT;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        settlements.Add(MapSettlement(reader));
                    }
                }
            }
            return settlements;
        }

        // --- LINQ QUERY METHODS ---

        public MonthlySettlement? GetById(int settlementId)
        {
            return GetAll().FirstOrDefault(s => s.SettlementId == settlementId);
        }

        public List<MonthlySettlement> GetByRenterId(int renterId)
        {
            return GetAll().Where(s => s.RenterId == renterId).ToList();
        }

        // --- CRUD OPERATIONS ---

        public void Add(MonthlySettlement settlement)
        {
            string sql = @"INSERT INTO dbo.MONTHLY_SETTLEMENT 
                           (RenterId, Month, TotalSales, Commission, TotalShelfRent, ShelfCount, ExtraDiscount, FinalAmount, IsProcessed)
                           VALUES (@RenterId, @Month, @TotalSales, @Commission, @TotalShelfRent, @ShelfCount, @ExtraDiscount, @FinalAmount, @IsProcessed);
                           SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RenterId", settlement.RenterId);
                command.Parameters.AddWithValue("@Month", settlement.Month);
                command.Parameters.AddWithValue("@TotalSales", settlement.TotalSales);
                command.Parameters.AddWithValue("@Commission", settlement.Commission);
                command.Parameters.AddWithValue("@TotalShelfRent", settlement.TotalShelfRent);
                command.Parameters.AddWithValue("@ShelfCount", settlement.ShelfCount);
                command.Parameters.AddWithValue("@ExtraDiscount", settlement.ExtraDiscount);
                command.Parameters.AddWithValue("@FinalAmount", settlement.FinalAmount);
                command.Parameters.AddWithValue("@IsProcessed", settlement.IsProcessed);

                connection.Open();

                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    settlement.SettlementId = Convert.ToInt32(result);
                }
            }
        }

        public void Update(MonthlySettlement settlement)
        {
            string sql = @"UPDATE dbo.MONTHLY_SETTLEMENT
                           SET RenterId = @RenterId,
                               Month = @Month,
                               TotalSales = @TotalSales,
                               Commission = @Commission,
                               TotalShelfRent = @TotalShelfRent,
                               ShelfCount = @ShelfCount,
                               ExtraDiscount = @ExtraDiscount,
                               FinalAmount = @FinalAmount,
                               IsProcessed = @IsProcessed
                           WHERE SettlementId = @SettlementId;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@SettlementId", settlement.SettlementId);
                command.Parameters.AddWithValue("@RenterId", settlement.RenterId);
                command.Parameters.AddWithValue("@Month", settlement.Month);
                command.Parameters.AddWithValue("@TotalSales", settlement.TotalSales);
                command.Parameters.AddWithValue("@Commission", settlement.Commission);
                command.Parameters.AddWithValue("@TotalShelfRent", settlement.TotalShelfRent);
                command.Parameters.AddWithValue("@ShelfCount", settlement.ShelfCount);
                command.Parameters.AddWithValue("@ExtraDiscount", settlement.ExtraDiscount);
                command.Parameters.AddWithValue("@FinalAmount", settlement.FinalAmount);
                command.Parameters.AddWithValue("@IsProcessed", settlement.IsProcessed);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private MonthlySettlement MapSettlement(SqlDataReader reader)
        {
            return new MonthlySettlement(
                Convert.ToInt32(reader["SettlementId"]),
                Convert.ToInt32(reader["RenterId"]),
                Convert.ToInt32(reader["Month"]),
                Convert.ToDouble(reader["TotalSales"]),
                Convert.ToDouble(reader["Commission"]),
                Convert.ToDouble(reader["TotalShelfRent"]),
                Convert.ToInt32(reader["ShelfCount"]),
                Convert.ToDouble(reader["ExtraDiscount"]),
                Convert.ToDouble(reader["FinalAmount"]),
                Convert.ToBoolean(reader["IsProcessed"])
            );
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Repositories
{
    public class ShelfRenterRepository : IShelfRenterRepository
    {
        private readonly string _connectionString;

        public ShelfRenterRepository()
        {
            _connectionString = @"Server=localhost;Database=ReolmarkedDb;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public ShelfRenterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Base fetch method retrieving all records directly via ADO.NET
        public List<ShelfRenter> GetAll()
        {
            List<ShelfRenter> renters = new List<ShelfRenter>();
            string sql = "SELECT RenterId, FirstName, LastName, PhoneNumber, Balance FROM dbo.SHELFRENTER;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        renters.Add(MapRenter(reader));
                    }
                }
            }
            return renters;
        }

        // --- LINQ-REFACTORED METHODS ---

        // GetById rewritten using LINQ FirstOrDefault
        public ShelfRenter? GetById(int renterId)
        {
            return GetAll().FirstOrDefault(r => r.RenterId == renterId);
        }

        // GetByName / Search rewritten using LINQ Where + Contains
        public List<ShelfRenter> GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return GetAll();

            return GetAll()
                .Where(r => (r.FirstName != null && r.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase)) ||
                            (r.LastName != null && r.LastName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // GetByPhoneNumber rewritten using LINQ Where
        public List<ShelfRenter> GetByPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return GetAll();

            return GetAll()
                .Where(r => r.PhoneNumber != null && r.PhoneNumber.Contains(phoneNumber))
                .ToList();
        }

        // --- CRUD METHODS ---

        public void Add(ShelfRenter renter)
        {
            string sql = @"INSERT INTO dbo.SHELFRENTER (FirstName, LastName, PhoneNumber, Balance)
                           VALUES (@FirstName, @LastName, @PhoneNumber, @Balance);
                           SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastName", renter.LastName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PhoneNumber", renter.PhoneNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Balance", renter.Balance);

                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    renter.RenterId = Convert.ToInt32(result);
                }
            }
        }

        public void Update(ShelfRenter renter)
        {
            string sql = @"UPDATE dbo.SHELFRENTER
                           SET FirstName = @FirstName,
                               LastName = @LastName,
                               PhoneNumber = @PhoneNumber,
                               Balance = @Balance
                           WHERE RenterId = @RenterId;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RenterId", renter.RenterId);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastName", renter.LastName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PhoneNumber", renter.PhoneNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Balance", renter.Balance);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int renterId)
        {
            string sql = @"DELETE FROM dbo.SHELFRENTER WHERE RenterId = @RenterId;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RenterId", renterId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private ShelfRenter MapRenter(SqlDataReader reader)
        {
            return new ShelfRenter(
                Convert.ToInt32(reader["RenterId"]),
                reader["FirstName"]?.ToString() ?? string.Empty,
                reader["LastName"]?.ToString() ?? string.Empty,
                reader["PhoneNumber"]?.ToString() ?? string.Empty,
                Convert.ToDouble(reader["Balance"])
            );
        }
    }
}
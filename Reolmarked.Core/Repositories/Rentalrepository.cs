// SRP: This class is responsible for persisting Rental rows to SQL Server.
// Implements every method from IRentalRepository.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Repositories
{
    // Handles all SQL for Rental objects.
    public class RentalRepository : IRentalRepository
    {
        private readonly string _connectionString;

        public RentalRepository()
        {
            _connectionString = @"Server=localhost;Database=ReolmarkedDb;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public RentalRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Base fetch method retrieving all records directly via ADO.NET
        public List<Rental> GetAll()
        {
            List<Rental> rentals = new List<Rental>();
            string sql = "SELECT RentalId, ShelfNumber, StartDate, EndDate, RenterId FROM dbo.RENTAL;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(MapRental(reader));
                    }
                }
            }
            return rentals;
        }

        // --- LINQ-REFACTORED METHODS ---

        // GetById using LINQ FirstOrDefault
        public Rental? GetById(int rentalId)
        {
            return GetAll().FirstOrDefault(r => r.RentalId == rentalId);
        }

        // GetByShelfNumber using LINQ FirstOrDefault
        public Rental? GetByShelfNumber(int shelfNumber)
        {
            return GetAll().FirstOrDefault(r => r.ShelfNumber == shelfNumber);
        }

        // GetByRenterId using LINQ Where
        public List<Rental> GetByRenterId(int renterId)
        {
            return GetAll().Where(r => r.RenterId == renterId).ToList();
        }

        // --- CRUD METHODS ---

        // Inserts one new rental row into the RENTAL table and populates its generated RentalId.
        public void Add(Rental rental)
        {
            string sql = @"INSERT INTO dbo.RENTAL (ShelfNumber, StartDate, EndDate, RenterId)
                           VALUES (@ShelfNumber, @StartDate, @EndDate, @RenterId);
                           SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShelfNumber", rental.ShelfNumber);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@EndDate", (object?)rental.EndDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@RenterId", rental.RenterId);

                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    rental.RentalId = Convert.ToInt32(result);
                }
            }
        }

        // Updates an existing rental identified by RentalId.
        public void Update(Rental rental)
        {
            string sql = @"UPDATE dbo.RENTAL
                           SET ShelfNumber = @ShelfNumber,
                               StartDate   = @StartDate,
                               EndDate     = @EndDate,
                               RenterId    = @RenterId
                           WHERE RentalId  = @RentalId;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RentalId", rental.RentalId);
                command.Parameters.AddWithValue("@ShelfNumber", rental.ShelfNumber);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@EndDate", (object?)rental.EndDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@RenterId", rental.RenterId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Deletes a rental by RentalId PK.
        public void Delete(int rentalId)
        {
            string sql = @"DELETE FROM dbo.RENTAL WHERE RentalId = @RentalId;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RentalId", rentalId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Helper: turns one row from the database into a C# Rental object.
        private Rental MapRental(SqlDataReader reader)
        {
            int rentalId = Convert.ToInt32(reader["RentalId"]);
            int shelfNumber = Convert.ToInt32(reader["ShelfNumber"]);
            DateTime startDate = Convert.ToDateTime(reader["StartDate"]);
            DateTime? endDate = reader["EndDate"] != DBNull.Value ? Convert.ToDateTime(reader["EndDate"]) : null;
            int renterId = Convert.ToInt32(reader["RenterId"]);

            return new Rental(rentalId, shelfNumber, startDate, endDate, renterId);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Reolmarked.Core.Interfaces;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Repositories
{
    public class ShelfRepository : IShelfRepository
    {
        private readonly string _connectionString;

        public ShelfRepository()
        {
            _connectionString = @"Server=localhost;Database=ReolmarkedDb;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public ShelfRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Base fetch method retrieving all records directly via ADO.NET
        public List<Shelf> GetAll()
        {
            List<Shelf> shelves = new List<Shelf>();
            string sql = "SELECT ShelfNumber, Status, ShelfConfiguration FROM dbo.SHELF;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        shelves.Add(MapShelf(reader));
                    }
                }
            }
            return shelves;
        }

        // --- LINQ-REFACTORED METHODS ---

        // GetById rewritten using LINQ FirstOrDefault
        public Shelf? GetById(int shelfNumber)
        {
            return GetAll().FirstOrDefault(s => s.ShelfNumber == shelfNumber);
        }

        // Filter shelves by Status using LINQ Where
        public List<Shelf> GetByStatus(Status status)
        {
            return GetAll().Where(s => s.Status == status).ToList();
        }

        // Filter shelves by Configuration using LINQ Where
        public List<Shelf> GetByConfiguration(ShelfConfiguration configuration)
        {
            return GetAll().Where(s => s.ShelfConfiguration == configuration).ToList();
        }

        // --- CRUD METHODS ---

        public void Add(Shelf shelf)
        {
            // 1. Calculate the next ShelfNumber using LINQ
            int nextShelfNumber = (GetAll().Select(s => (int?)s.ShelfNumber).Max() ?? 0) + 1;

            // 2. Enable IDENTITY_INSERT so SQL Server allows passing an explicit ID
            string sql = """
                -- 1. Reseed the counter to MAX(ShelfNumber) currently in the table
                DBCC CHECKIDENT ('dbo.SHELF', RESEED);

                -- 2. Insert row (SQL Server automatically assigns the next sequential ShelfNumber)
                INSERT INTO dbo.SHELF (Status, ShelfConfiguration)
                VALUES (@Status, @ShelfConfiguration);
                """;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@ShelfNumber", nextShelfNumber);
                command.Parameters.AddWithValue("@Status", shelf.Status.ToString());
                command.Parameters.AddWithValue("@ShelfConfiguration", shelf.ShelfConfiguration.ToString());

                connection.Open();
                command.ExecuteNonQuery();
            }

            // Assign the incremented ID back to the object in memory
            shelf.ShelfNumber = nextShelfNumber;
        }

        public void Update(Shelf shelf)
        {
            string sql = @"UPDATE dbo.SHELF
                           SET Status = @Status,
                               ShelfConfiguration = @ShelfConfiguration
                           WHERE ShelfNumber = @ShelfNumber;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShelfNumber", shelf.ShelfNumber);
                command.Parameters.AddWithValue("@Status", shelf.Status.ToString());
                command.Parameters.AddWithValue("@ShelfConfiguration", shelf.ShelfConfiguration.ToString());

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Deletes the shelf row. No reseeding — we keep the identity counter as-is
        // so foreign keys in RENTAL still point to the correct shelf.
        public void Delete(int shelfNumber)
        {
            string sql = @"DELETE FROM dbo.SHELF WHERE ShelfNumber = @ShelfNumber;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShelfNumber", shelfNumber);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private Shelf MapShelf(SqlDataReader reader)
        {
            int shelfNumber = Convert.ToInt32(reader["ShelfNumber"]);

            // Parse Status enum from database string
            string? statusStr = reader["Status"]?.ToString();
            Status status = Enum.TryParse<Status>(statusStr, out var parsedStatus)
                ? parsedStatus
                : Status.Ledig;

            // Parse Configuration enum from database string
            string? configStr = reader["ShelfConfiguration"]?.ToString();
            ShelfConfiguration config = Enum.TryParse<ShelfConfiguration>(configStr, out var parsedConfig)
                ? parsedConfig
                : ShelfConfiguration.SeksHylder;

            return new Shelf(shelfNumber, config, status);
        }
    }
}
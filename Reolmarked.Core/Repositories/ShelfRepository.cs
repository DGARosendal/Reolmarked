using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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

        public void Add(Shelf shelf)
        {
            string sql = @"INSERT INTO dbo.SHELF (Status, Configuration)
                           VALUES (@Status, @Configuration);
                           SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);

                // Convert enum values to string for database storage
                command.Parameters.AddWithValue("@Status", shelf.Status.ToString());
                command.Parameters.AddWithValue("@Configuration", shelf.Configuration.ToString());

                connection.Open();

                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    // Set the auto-generated identity ID back on the object
                    shelf.ShelfNumber = Convert.ToInt32(result);
                }
            }
        }

        public void Update(Shelf shelf)
        {
            string sql = @"UPDATE dbo.SHELF
                           SET Status = @Status,
                               Configuration = @Configuration
                           WHERE ShelfNumber = @ShelfNumber;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShelfNumber", shelf.ShelfNumber);
                command.Parameters.AddWithValue("@Status", shelf.Status.ToString());
                command.Parameters.AddWithValue("@Configuration", shelf.Configuration.ToString());

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

        public List<Shelf> GetAll()
        {
            List<Shelf> shelves = new List<Shelf>();
            string sql = "SELECT ShelfNumber, Status, Configuration FROM dbo.SHELF;";

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

        public Shelf GetById(int shelfNumber)
        {
            string sql = "SELECT ShelfNumber, Status, Configuration FROM dbo.SHELF WHERE ShelfNumber = @ShelfNumber;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShelfNumber", shelfNumber);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapShelf(reader);
                    }
                }
            }
            return null;
        }

        private Shelf MapShelf(SqlDataReader reader)
        {
            int shelfNumber = Convert.ToInt32(reader["ShelfNumber"]);

            // Parse Status enum from database string
            string statusStr = reader["Status"]?.ToString();
            Status status = Enum.TryParse<Status>(statusStr, out var parsedStatus)
                ? parsedStatus
                : Status.Ledig;

            // Parse Configuration enum from database string
            string configStr = reader["Configuration"]?.ToString();
            Configuration config = Enum.TryParse<Configuration>(configStr, out var parsedConfig)
                ? parsedConfig
                : Configuration.SeksHylder;

            return new Shelf(shelfNumber, config, status);
        }
    }
}
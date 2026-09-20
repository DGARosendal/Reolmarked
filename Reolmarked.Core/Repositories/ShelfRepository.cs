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
            // Do NOT insert ShelfNumber explicitly because SQL Server handles it automatically
            string sql = @"INSERT INTO dbo.SHELF (Status, Configuration)
                   VALUES (@Status, @Configuration);
                   SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Status", shelf.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Configuration", shelf.Configuration ?? (object)DBNull.Value);

                connection.Open();

                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    // Assign the auto-generated identity ID back to the object
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
                command.Parameters.AddWithValue("@Status", shelf.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Configuration", shelf.Configuration ?? (object)DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int shelfNumber)
        {
            // 1. Delete the specified shelf row
            string deleteSql = @"DELETE FROM dbo.SHELF WHERE ShelfNumber = @ShelfNumber;";

            // 2. Reseed the IDENTITY column to the highest remaining ShelfNumber (or 0 if table is empty)
            string reseedSql = @"
        DECLARE @maxId INT = (SELECT ISNULL(MAX(ShelfNumber), 0) FROM dbo.SHELF);
        DBCC CHECKIDENT ('dbo.SHELF', RESEED, @maxId);";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand deleteCmd = new SqlCommand(deleteSql, connection))
                {
                    deleteCmd.Parameters.AddWithValue("@ShelfNumber", shelfNumber);
                    deleteCmd.ExecuteNonQuery();
                }

                using (SqlCommand reseedCmd = new SqlCommand(reseedSql, connection))
                {
                    reseedCmd.ExecuteNonQuery();
                }
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
            return new Shelf
            {
                ShelfNumber = Convert.ToInt32(reader["ShelfNumber"]),
                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                Configuration = reader["Configuration"] != DBNull.Value ? reader["Configuration"].ToString() : null
            };
        }
    }
}
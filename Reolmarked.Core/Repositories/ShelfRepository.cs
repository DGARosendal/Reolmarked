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

        // Collections for Enums
        private Dictionary<int, ShelfConfiguration> _shelfConfigurations = new Dictionary<int, ShelfConfiguration>();
        private Dictionary<int, Status> _statuses = new Dictionary<int, Status>();

        public ShelfRepository(string connectionString)
        {
            _connectionString = connectionString;
            // Populate collections for Enums
            SetShelfConfigurations();
            SetStatuses();
        }

        // Base fetch method retrieving all records directly via ADO.NET
        public List<Shelf> GetAll()
        {
            List<Shelf> shelves = new List<Shelf>();
            string sql = "SELECT ShelfNumber, StatusId, ShelfConfigurationId FROM dbo.SHELF;";

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
                INSERT INTO dbo.SHELF (StatusId, ShelfConfigurationId)
                VALUES (@StatusId, @ShelfConfigurationId);
                """;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@ShelfNumber", nextShelfNumber);
                // Convert enum values to string for database storage
                command.Parameters.AddWithValue("@StatusId", 1 + (int)shelf.Status);
                command.Parameters.AddWithValue("@ShelfConfigurationId", 1 + (int)shelf.ShelfConfiguration);

                connection.Open();
                command.ExecuteNonQuery();
            }

            // Assign the incremented ID back to the object in memory
            shelf.ShelfNumber = nextShelfNumber;
        }

        public void Update(Shelf shelf)
        {
            string sql = @"UPDATE dbo.SHELF
                           SET StatusId = @StatusId,
                               ShelfConfigurationId = @ShelfConfigurationId
                           WHERE ShelfNumber = @ShelfNumber;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShelfNumber", shelf.ShelfNumber);
                command.Parameters.AddWithValue("@StatusId", 1 + (int)shelf.Status);
                command.Parameters.AddWithValue("@ShelfConfigurationId", 1 + (int)shelf.ShelfConfiguration);

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
            int shelfConfigurationId = Convert.ToInt32(reader["ShelfConfigurationId"]);
            int statusId = Convert.ToInt32(reader["StatusId"]);

            ShelfConfiguration config;
            if (_shelfConfigurations.ContainsKey(shelfConfigurationId))
            {
                config = _shelfConfigurations[shelfConfigurationId];
            }
            else
            {
                // Errorhandling missing
                config = _shelfConfigurations[1];
            }
            Status status;
            if (_statuses.ContainsKey(statusId))
            {
                status = _statuses[statusId];
            }
            else
            {
                // Errorhandling missing
                status = _statuses[1];
            }


            return new Shelf(shelfNumber, config, status);
        }

        /// <summary>
        /// Gets all ShelfConfigurations from the Enum table and adds them to _shelfConfigurations
        /// </summary>
        private void SetShelfConfigurations()
        {
            string sql = "SELECT ShelfConfigurationId, ShelfConfigurationText FROM dbo.SHELFCONFIGURATION;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        (int Id, ShelfConfiguration config) read = MapShelfConfiguration(reader);
                        _shelfConfigurations.Add(read.Id, read.config);
                    }
                }
            }

        }

        /// <summary>
        /// Reads from a SqlDataReader and outputs a ShelfConfiguration and it's Id in the Enum table
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private (int, ShelfConfiguration) MapShelfConfiguration(SqlDataReader reader)
        {
            int shelfConfigurationId = Convert.ToInt32(reader["ShelfConfigurationId"]);

            // Errorhandling missing
            // Parse ShelfConfiguration enum from database string
            string? configStr = reader["ShelfConfigurationText"]?.ToString();
            ShelfConfiguration config = Enum.TryParse<ShelfConfiguration>(configStr, out var parsedConfig)
                ? parsedConfig
                : ShelfConfiguration.SeksHylder;

            return (shelfConfigurationId, config);
        }

        /// <summary>
        /// Gets all Statuses from the Enum table and adds them to _statuses
        /// </summary>
        private void SetStatuses()
        {
            string sql = "SELECT StatusId, StatusText FROM dbo.[STATUS];";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        (int Id, Status status) read = MapStatus(reader);
                        _statuses.Add(read.Id, read.status);
                    }
                }
            }
        }

        /// <summary>
        /// Reads from a SqlDataReader and outputs a Status and it's Id in the Enum table
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private (int, Status) MapStatus(SqlDataReader reader)
        {
            int statusId = Convert.ToInt32(reader["StatusId"]);

            // Errorhandling missing
            // Parse Status enum from database string
            string? statusStr = reader["StatusText"]?.ToString();
            Status status = Enum.TryParse<Status>(statusStr, out var parsedStatus)
                ? parsedStatus
                : Status.Ledig;

            return (statusId, status);
        }
    }
}
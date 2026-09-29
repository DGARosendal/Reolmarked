using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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

        #region ============= CRUD =============
        public void Add(ShelfRenter renter)
        {
            string sql = @"INSERT INTO dbo.SHELFRENTER (FirstName, LastName, PhoneNumber)
                           VALUES (@FirstName, @LastName, @PhoneNumber);
                           SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", renter.PhoneNumber);

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
                               PhoneNumber = @PhoneNumber
                           WHERE RenterId = @RenterId;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RenterId", renter.RenterId);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", renter.PhoneNumber);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int renterId)
        {
            string sql = @"
            BEGIN TRANSACTION;

            -- 1. Slet den valgte reollejer
            DELETE FROM dbo.SHELFRENTER WHERE RenterId = @RenterId;

            -- 2. Gem de resterende reollejere i en midlertidig tabel i ID-rækkefølge
            SELECT FirstName, LastName, PhoneNumber
            INTO #TempRenters
            FROM dbo.SHELFRENTER
            ORDER BY RenterId;

            -- 3. Tøm tabellen (dette nulstiller IDENTITY-tælleren til 1)
            TRUNCATE TABLE dbo.SHELFRENTER;

            -- 4. Genindsæt alle reollejere så de tildeles fortløbende ID'er (1, 2, 3...)
            INSERT INTO dbo.SHELFRENTER (FirstName, LastName, PhoneNumber)
            SELECT FirstName, LastName, PhoneNumber
            FROM #TempRenters;

            DROP TABLE #TempRenters;

            COMMIT TRANSACTION;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RenterId", renterId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<ShelfRenter> GetAll()
        {
            List<ShelfRenter> renters = new List<ShelfRenter>();
            string sql = "SELECT RenterId, FirstName, LastName, PhoneNumber FROM dbo.SHELFRENTER;";

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


        #endregion


        /// <summary>
        /// Method uses SQL string to get Renter by its Id. 
        /// </summary>
        /// <param name="renterId"></param>
        /// <returns></returns>
        public ShelfRenter GetById(int renterId)
        {
            string sql = "SELECT RenterId, FirstName, LastName, PhoneNumber FROM dbo.SHELFRENTER WHERE RenterId = @RenterId;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RenterId", renterId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapRenter(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Maps data from query to Renter class creating and returning a new Shelfrenter object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ShelfRenter MapRenter(SqlDataReader reader)
        {
            return new ShelfRenter(
                Convert.ToInt32(reader["RenterId"]),
                reader["FirstName"]?.ToString(),
                reader["LastName"]?.ToString(),
                reader["PhoneNumber"]?.ToString()
            );
        }
    }
}
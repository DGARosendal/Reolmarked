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

        // Deletes the renter row. No TRUNCATE — we keep the identity counter as-is
        // so foreign keys in RENTAL still point to the correct renter.
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
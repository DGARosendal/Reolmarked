// SRP: This class is responsible for persisting Rental rows to SQL Server.
// Implements every method from IRentalRepository. 

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Reolmarked.Core.Models;

namespace Reolmarked.Core.Repositories
{
    // Handles all SQL for Rental objects.
    public class RentalRepository : IRentalRepository
    {
        // Tells SqlClient where the database lives and how to log in.
        private readonly string _connectionString;

        public RentalRepository()
        {
            // Connection string that tells us that we are utilizing a database called ReolmarkedDb
            // residing on the localhost, i.e. our own machine, where we log in via the current
            // Windows user (Trusted_Connection=True), and also bypass the certificate validation,
            // as we trust our local machine (TrustServerCertificate=True).
            _connectionString = @"Server=localhost;Database=ReolmarkedDb;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        // Inserts one new rental row into the RENTAL table.
        // Fails if the shelf already has a rental (PK constraint).
        public void Add(Rental rental)
        {
            // The @ before the quote makes this a verbatim string (keeps line breaks).
            // The @ inside the SQL marks parameters, filled in below via AddWithValue.
            string sql = @"INSERT INTO dbo.RENTAL (ShelfNumber, StartDate, RenterId, UserId)
               VALUES (@ShelfNumber, @StartDate, @RenterId, @UserId);";

            // using closes the connection automatically when the block ends.
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // SqlCommand holds the SQL text and the connection it should run on.
                // Nothing is sent yet, the command is just prepared here.
                SqlCommand command = new SqlCommand(sql, connection);

                // Bind each @-placeholder to a value from the Rental object in our code.
                command.Parameters.AddWithValue("@ShelfNumber", rental.ShelfNumber);

                // StartDate is a DateTime; AddWithValue handles the format for us in our code.
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);

                // RenterId points to an existing renter in the SHELFRENTER table in our code.
                command.Parameters.AddWithValue("@RenterId", rental.RenterId);

                // UserId is the employee who created this rental (for auditing) in our code.
                command.Parameters.AddWithValue("@UserId", rental.UserId);

                // Open the connection right before we need it.
                connection.Open();

                // ExecuteNonQuery runs a DML statement (INSERT/UPDATE/DELETE) and does
                // not return rows. Here we run an INSERT (the Add operation), inserting
                // one rental into the RENTAL table. This is where the SQL text and the
                // parameter values are sent to the database.
                command.ExecuteNonQuery();
            }
        }

        // Updates an existing rental. Identified by ShelfNumber (PK).
        public void Update(Rental rental)
        {
            // Another verbatim string like above, where the values from the code
            // get updated below via AddWithValue. Because we are looking to update
            // the RENTAL table, we will have a ShelfNumber (PK) to act on
            // ("WHERE Shelfnumber = @ShelfNumber"). Without the WHERE, we would end up 
            // updating all shelves. 
            string sql = @"UPDATE dbo.RENTAL
                           SET StartDate = @StartDate,
                               RenterId  = @RenterId,
                               UserId    = @UserId
                           WHERE ShelfNumber = @ShelfNumber;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShelfNumber", rental.ShelfNumber);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@RenterId", rental.RenterId);
                command.Parameters.AddWithValue("@UserId", rental.UserId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Deletes the rental for a given shelf.
        // Unlike SHELF and SHELFRENTER, the RENTAL table has no IDENTITY column.
        // Its PK (ShelfNumber) is just a reference to a shelf, so there is
        // no counter (IDENTITY) to reset when a rental is deleted.
        public void Delete(int shelfNumber)
        {
            // Verbatim SQL command that deletes the row in the RENTAL table
            // whose PK (ShelfNumber) matches the given value.
            // In other words: deletes the rental for a given shelf.
            string sql = @"DELETE FROM dbo.RENTAL WHERE ShelfNumber = @ShelfNumber;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                // Fill in the actual shelf number to identify which rental to delete.
                // We use an int here (not a Rental object) because ShelfNumber is the
                // PK, and the PK is enough to find one specific row.
                command.Parameters.AddWithValue("@ShelfNumber", shelfNumber);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Returns every rental in the database.
        public List<Rental> GetAll()
        {
            // We initialize a list of rentals to work with in the code that can hold all the rentals we get.
            List<Rental> rentals = new List<Rental>();

            // The data we want to select is the data in the columns ShelfNumber, StartDate,
            // RenterId and UserId from the RENTAL table, i.e. we want the full RENTAL entry
            // from the database, which we will gather by iterating below.
            string sql = "SELECT ShelfNumber, StartDate, RenterId, UserId FROM dbo.RENTAL;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                // This method is different from the previous operations, because we are
                // no longer using DML (INSERT/UPDATE/DELETE) to modify data. Here we use
                // DQL (SELECT) to read data back, so we call ExecuteReader instead of
                // ExecuteNonQuery. ExecuteReader returns an SqlDataReader, which we store
                // in the variable "reader".
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // While there are still rows to read, we keep reading.
                    while (reader.Read())
                    {
                        // Each row is converted to a proper C# Rental object via the
                        // MapRental method, then added to the list we initialized above.
                        rentals.Add(MapRental(reader));
                    }
                }
            }
            return rentals;
        }
        // Returns one rental by shelf number, or null if none exists. 
        public Rental GetByShelfNumber(int shelfNumber)
        {
            // We select the data we need from the RENTAL table based on the shelfNumber-parameter from above.
            string sql = "SELECT ShelfNumber, StartDate, RenterId, UserId FROM dbo.RENTAL WHERE ShelfNumber = @ShelfNumber;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShelfNumber", shelfNumber);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapRental(reader);
                    }
                }
            }
            return null;
        }

        // Returns every rental for one renter.
        public List<Rental> GetByRenterId(int renterId)
        {
            List<Rental> rentals = new List<Rental>();
            string sql = "SELECT ShelfNumber, StartDate, RenterId, UserId FROM dbo.RENTAL WHERE RenterId = @RenterId;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@RenterId", renterId);
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

        // Helper: turns one row from the database into a C# Rental object.
        // Columns are read by name and converted to the proper C# types,
        // since SqlDataReader always returns values as object.
        private Rental MapRental(SqlDataReader reader)
        {
            return new Rental(
                Convert.ToInt32(reader["ShelfNumber"]),
                Convert.ToDateTime(reader["StartDate"]),
                Convert.ToInt32(reader["RenterId"]),
                Convert.ToInt32(reader["UserId"])
            );
        }
    }
}
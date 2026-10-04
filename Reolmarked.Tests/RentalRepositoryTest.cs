using System;
using System.Collections.Generic;
using System.Text;
using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;
using Microsoft.Data.SqlClient;

namespace Reolmarked.Tests
{
    [TestClass]
    public class RentalRepositoryTest
    {
        [TestMethod]
        public void Add_WhenShelfAlreadyHasRental_ShouldThrowSqlException()
        {
            //Arrange: Forbered en reol, en reollejer og to udlejninger
            RentalRepository rentalRepository = new RentalRepository();

            //Opretter testreollejeren, der endnu ikke er gemt i vores database
            ShelfRenter renter = new ShelfRenter("Test", "Testesen", "12345678");

            //Opretter et repository til reollejere
            ShelfRenterRepository renterRepository = new ShelfRenterRepository();

            //Gemmer testreollejeren i databasen
            renterRepository.Add(renter);

            //Opretter et repository til reoler
            ShelfRepository shelfRepository = new ShelfRepository();

            //Opretter testreolen med seks hylder og status ledig
            //Databasen erstatter 0 med det nye reolnummer, når vi gemmer
            Shelf shelf = new Shelf(1, ShelfConfiguration.SeksHylder, Status.Ledig);

            //Gemmer testreolen og sætter shelf.ShelfNumber til det nye nummer
            shelfRepository.Add(shelf);

            //Begge udlejninger bruger en gyldig startdato: 1. november 2026
            DateTime startDate = new DateTime(2026, 11, 1);

            //Opretter den første udlejning til vores testreol og testreollejer
            Rental firstRental = new Rental(shelf.ShelfNumber, startDate, renter.RenterId);

            //Opretter en anden udlejning med samme reolnummer
            Rental secondRental = new Rental(shelf.ShelfNumber, startDate, renter.RenterId);

            //Act: Gem den første udlejning
            rentalRepository.Add(firstRental);

            //Assert: Krontollér, at den anden udlejning afvises
            //Forsøger at gemme den anden udlejning på samme reol.
            SqlException exception = Assert.ThrowsExactly<SqlException>(() => rentalRepository.Add(secondRental));

            //Kontroller, at databasen afviser det gentagne reolnummer
            Assert.AreEqual(2627, exception.Number);

            //Cleanup: Fjern testdata, hvis testen når hertil
            //sletter testudlejningen først, fordi den henviser til reolen og reollejeren
            rentalRepository.Delete(shelf.ShelfNumber);

            //sletter testreolen
            shelfRepository.Delete(shelf.ShelfNumber);

            //sletter testreollejeren
            renterRepository.Delete(renter.RenterId);




        }
    }
}

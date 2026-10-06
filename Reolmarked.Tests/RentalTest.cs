using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reolmarked.Core.Models;
using System;

namespace Reolmarked.Tests
{
    [TestClass]
    public class RentalTest
    {
        [TestMethod]
        public void Constructor_WithValidArguments_ShouldCreateRental()
        {
            // Arrange: Vælg en gyldig startdato.
            DateTime startDate = new DateTime(2026, 11, 1);

        // Act: Opret en udlejning med gyldige argumenter. 
        Rental rental = new Rental(1, startDate, 1);

            // Assert: Kontroller, at startdatoen blev oprettet korrekt.
            Assert.AreEqual(startDate, rental.StartDate);
        }

        [TestMethod]
        public void Constructor_StartDateNotOnFirst_ShouldThrowArgumentException()
        {
            //Arrange: Vælg en startdato, der ikke er den første i måneden.
            DateTime startDate = new DateTime(2026, 11, 15);

            //Act & Assert: Forsøg at oprette en udlejning, // og forvent, at der kastes en ArgumentException.
            Assert.ThrowsExactly<ArgumentException>(() => new Rental(1, startDate, 1));
        }

        [TestMethod]
        public void StartDate_AfterEndDate_ShouldThrowArgumentException()
        {
            //Arrange: Opret en udlejning med en gyldig startdato og slutdato.
            Rental rental = new Rental(
                shelfNumber: 1,
                startDate: new DateTime(2026, 11, 1),
                renterId: 1,
                endDate: new DateTime(2026, 11, 30));

            //Act & Assert: Forsøg at sætte startdatoen til en dato efter slutdatoen
            Assert.ThrowsExactly<ArgumentException>(() => rental.StartDate = new DateTime(2026, 12, 1));

        }
    }
}

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
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reolmarked.Core.Models;
using System;

using System.Configuration;

namespace Reolmarked.Tests
{
    [TestClass]
    public sealed class ShelfTests
    {
        [TestMethod]
        public void Constructor_WithValidArguments_ShouldCreateShelf()
        {
            // Arrange & Act
            // var shelf = new Shelf(1, Configuration, Status.Ledig);
            var newShelf = new Shelf(1, Core.Models.Configuration.TreHylderOgBøjle, Status.Ledig);

            // Assert
            Assert.AreEqual(1, newShelf.ShelfNumber);
            Assert.AreEqual(Core.Models.Configuration.TreHylderOgBøjle, newShelf.Configuration);
            Assert.AreEqual(Status.Ledig, newShelf.Status);
        }

        /*
        [TestMethod]
        public void Constructor_WithInValidArguments_ShouldCreateShelf()
        {
            // Arrange & Act
            // var shelf = new Shelf(1, Configuration, Status.Ledig);
            var newShelf = new Shelf(1, Core.Models.Configuration.TreHylderOgBøjle, (Status)2);

            // Assert
            Assert.AreEqual(1, newShelf.ShelfNumber);
            Assert.AreEqual(Core.Models.Configuration.TreHylderOgBøjle, newShelf.Configuration);
            Assert.AreNotEqual(Status.Ledig, newShelf.Status);
        }
        */
    }

}
using Reolmarked.Core.Models;

namespace Reolmarked.Tests;

[TestClass]
public class ShelRenterTests
{
    [TestMethod]
    public void Constructor_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var newRenter = new ShelfRenter(1, " Jonas ", "  Berg", "+45 62-33-56-78 ");

        // Assert
        Assert.AreEqual(1, newRenter.RenterId);
        Assert.AreEqual("Jonas", newRenter.FirstName);
        Assert.AreEqual("Berg", newRenter.LastName);
        Assert.AreEqual("+4562335678", newRenter.PhoneNumber);

    }

    [TestMethod]
    public void ValidateName_CheckForThrowsException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => ShelfRenter.ValidateName("9Jens", "Fornavn"));
        Assert.ThrowsExactly<ArgumentException>(() => ShelfRenter.ValidateName(null, "Fornavn"));
        Assert.ThrowsExactly<ArgumentException>(() => ShelfRenter.ValidateName("Jensen#", "Efternavn"));
    }



}

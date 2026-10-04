using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;

namespace Reolmarked.Tests;

[TestClass]
public class ShelfRepositoryIntegrationTests : IntegrationTestBase
{
    #region CRUD
        
    [TestMethod]
    public void AddAndGetById_ShouldPersistShelfToDataBase()
    {
        // Arrange
        var newShelf = new Shelf(1, ShelfConfiguration.SeksHylder, Status.Ledig);

        // Act
        _repository.Add(newShelf);
        int generatedId = newShelf.ShelfNumber;

        var fetchedShelf = _repository.GetById(generatedId);

        // Assert
        Assert.IsNotNull(fetchedShelf);
        Assert.AreEqual(generatedId, fetchedShelf.ShelfNumber);
        Assert.AreEqual(ShelfConfiguration.SeksHylder, fetchedShelf.ShelfConfiguration);
        Assert.AreEqual(Status.Ledig, fetchedShelf.Status);

        // Cleanup
        _repository.Delete(generatedId);
    }

    #endregion

    
}

using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.ViewModels;

namespace Reolmarked.Tests;

[TestClass]
public class ShelfViewModelTests
{
    private readonly string _testConnetionString = @"Server=localhost;Database=ReolmarkedTestDb;Trusted_Connection=True;TrustServerCertificate=True;";
    private ShelfRepository _repository;
    private ShelfViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _repository = new ShelfRepository(connectionString: _testConnetionString);
        _viewModel = new ShelfViewModel(_repository);
    }

    [TestMethod]
    public void InitialState_ShouldLoadShelvesAndSetDefault()
    {
        // Assert
        Assert.IsNotNull(_viewModel.Shelves);
        Assert.AreEqual(Status.Ledig, _viewModel.Status);
        Assert.AreEqual(ShelfConfiguration.SeksHylder, _viewModel.Configuration);
    }


    [TestMethod]
    public void SelectShelfCommand_ShouldUpdateSelectedShelf()
    {
        // Arrange
        var testShelf = new Shelf(5, ShelfConfiguration.TreHylderOgBøjle, Status.Booket);

        // Act
        _viewModel.SelectShelfCommand.Execute(testShelf);

        // Assert
        Assert.AreEqual(testShelf, _viewModel.SelectedShelf);
        Assert.AreEqual(5, _viewModel.ShelfNumber);
        Assert.AreEqual(Status.Booket, _viewModel.Status);
        Assert.AreEqual(ShelfConfiguration.TreHylderOgBøjle, _viewModel.Configuration);
    }

}

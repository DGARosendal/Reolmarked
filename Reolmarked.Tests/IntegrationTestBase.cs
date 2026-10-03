using Reolmarked.Core.Repositories;
using Reolmarked.UI.ViewModels;

namespace Reolmarked.Tests;

[TestClass]
public abstract class IntegrationTestBase
{
    protected readonly string _testConnetionString = @"Server=localhost;Database=ReolmarkedTestDb;Trusted_Connection=True;TrustServerCertificate=True;";
    protected ShelfRepository _repository;


    [TestInitialize]
    public void BaseSetup()
    {
        _repository = new ShelfRepository(connectionString: _testConnetionString);
    }
}

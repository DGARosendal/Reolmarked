
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Reolmarked.Core.Repositories;

namespace Reolmarked.Tests;

[TestClass]
public class DatabaseConnectionIntegrationTests : IntegrationTestBase
{

    // TODO Needs updating 
    /*[TestMethod]
    public void CanOpenDatabaseConnection_ShouldConnectSuccesfully()
    {
        // Act & Assert
        using (var connection = new SqlConnection(@"Server=localhost;Database=ReolmarkedDb;Trusted_Connection=True;TrustServerCertificate=True;"))
        {
            connection.Open();
            Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);
        }
    }
    */

    /// <summary>
    /// Checks if can connection succesfully to ReolmarkedTestDb
    /// 
    ///  The other IntegrationTests passes even when the databasee is disconnected, so make sure this passes before running integration tests.
    /// </summary>
    [TestMethod]
    public void CanOpenTestDatabaseConnection_ShouldConnectSuccesfully()
    {
        // Act & Assert
        using (var connection = new SqlConnection(_testConnetionString))
        {
            connection.Open();
            Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);
        }
    }
}

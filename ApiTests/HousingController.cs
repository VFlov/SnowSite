using Microsoft.AspNetCore.Mvc;
using Moq;
using Npgsql;
using SnowSite.Server.Controllers;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xunit;
using SnowSite.Server.Models;
namespace ApiTests;
public class HousingControllerTests
{
    [Fact]
    public void GetHouses_ReturnsOkResult_WithListOfHouses()
    {
        // Arrange
        var mockConnection = new Mock<NpgsqlConnection>();
        var mockCommand = new Mock<NpgsqlCommand>();
        var mockReader = new Mock<NpgsqlDataReader>();

        // Настройка моков для возврата данных
        mockReader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false); // Симулируем одну строку данных
        mockReader.Setup(r => r.GetString(0)).Returns("A");
        mockReader.Setup(r => r.GetDouble(1)).Returns(10.0);
        mockReader.Setup(r => r.GetDouble(2)).Returns(20.0);

         mockReader.Setup(r => r.GetDouble(It.IsAny<int>())).Returns((int column) =>
        {
            if (column == 1)
                return 10.0;
            else if (column == 2)
                return 20.0;
            throw new ArgumentOutOfRangeException(nameof(column));
        });
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        var controller = new HousingController();

        // Act
        var result = controller.GetHouses(0, 10, 0, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var houses = Assert.IsType<List<HouseData>>(okResult.Value);
        Assert.Single(houses);
        Assert.Equal("A", houses[0].IhsEeClass);
        Assert.Equal(10.0, houses[0].HplGeoLongitude);
        Assert.Equal(20.0, houses[0].HplGeoLatitude);
    }

    [Fact]
    public void GetHouses_ReturnsEmptyList_WhenNoData()
    {
        // Arrange
        var mockConnection = new Mock<NpgsqlConnection>();
        var mockCommand = new Mock<NpgsqlCommand>();
        var mockReader = new Mock<NpgsqlDataReader>();

        // Настройка моков для возврата пустого результата
        mockReader.Setup(r => r.Read()).Returns(false);

         mockReader.Setup(r => r.GetDouble(It.IsAny<int>())).Returns((int column) =>
        {
            if (column == 1)
                return 10.0;
            else if (column == 2)
                return 20.0;
            throw new ArgumentOutOfRangeException(nameof(column));
        });
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        var controller = new HousingController();

        // Act
        var result = controller.GetHouses(0, 10, 0, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var houses = Assert.IsType<List<HouseData>>(okResult.Value);
        Assert.Empty(houses);
    }
}
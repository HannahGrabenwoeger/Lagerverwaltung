using Xunit;
using Backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Backend.Dto;
using FirebaseAdmin.Auth;
using System.Collections.Generic;
using Backend.Dtos;

public class AuthentificationControllerTests
{
    private AuthentificationController CreateController(Mock<FirebaseAuth>? mockAuth = null)
    {
        return new AuthentificationController();
    }

    [Fact]
    public async Task VerifyFirebaseToken_ReturnsOk_WhenTokenValid()
    {
        // Arrange
        var controller = CreateController();
        var idToken = "valid-token";

        // Act
        var result = await controller.VerifyFirebaseToken(idToken);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task VerifyToken_ReturnsUid_WhenTokenValid()
    {
        // Arrange
        var controller = CreateController();
        var model = new FirebaseAuthDto
        {
            IdToken = "valid-token"
        };

        // Act
        var result = await controller.VerifyToken(model);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetUid_ReturnsUid_WhenTokenValid()
    {
        // Arrange
        var controller = CreateController();
        var token = "valid-token";

        // Act
        var result = await controller.GetUid(token);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
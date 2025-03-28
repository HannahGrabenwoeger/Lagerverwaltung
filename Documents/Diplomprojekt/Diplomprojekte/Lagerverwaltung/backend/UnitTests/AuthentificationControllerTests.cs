using Xunit;
using Backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Backend.Dto;
using FirebaseAdmin.Auth;
using System.Collections.Generic;
using Backend.Dtos;
using Backend.Services.Firebase;

public class AuthentificationControllerTests
{
    private AuthentificationController CreateController(Mock<IFirebaseAuthWrapper>? mockAuth = null)
    {
        return new AuthentificationController(mockAuth?.Object ?? Mock.Of<IFirebaseAuthWrapper>());
    }

    [Fact]
    public async Task VerifyFirebaseToken_ReturnsOk_WhenTokenValid()
    {
        var mockAuth = new Mock<IFirebaseAuthWrapper>();
        var mockToken = new Mock<FirebaseToken>();
        mockToken.Setup(t => t.Uid).Returns("mocked-uid");
        mockAuth.Setup(auth => auth.VerifyIdTokenAsync("valid-token"))
                .ReturnsAsync(mockToken.Object);

        var controller = CreateController(mockAuth);

        var result = await controller.VerifyFirebaseToken("valid-token");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("mocked-uid", okResult.Value);
    }

    [Fact]
    public async Task VerifyToken_ReturnsUid_WhenTokenValid()
    {
        var mockAuth = new Mock<IFirebaseAuthWrapper>();
        var mockToken = new Mock<FirebaseToken>();
        mockToken.Setup(t => t.Uid).Returns("mocked-uid");
        mockAuth.Setup(auth => auth.VerifyIdTokenAsync("valid-token"))
                .ReturnsAsync(mockToken.Object);

        var controller = CreateController(mockAuth);
        var model = new FirebaseAuthDto
        {
            IdToken = "valid-token"
        };

        var result = await controller.VerifyToken(model);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("mocked-uid", okResult.Value);
    }

    [Fact]
    public async Task GetUid_ReturnsUid_WhenTokenValid()
    {
        var mockAuth = new Mock<IFirebaseAuthWrapper>();
        var mockToken = new Mock<FirebaseToken>();
        mockToken.Setup(t => t.Uid).Returns("mocked-uid");
        mockAuth.Setup(auth => auth.VerifyIdTokenAsync("valid-token"))
                .ReturnsAsync(mockToken.Object);

        var controller = CreateController(mockAuth);
        var token = "valid-token";

        var result = await controller.GetUid(token);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("mocked-uid", okResult.Value);
    }
}
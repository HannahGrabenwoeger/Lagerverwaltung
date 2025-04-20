using Xunit;
using Backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Backend.Dtos;
using Backend.Services.Firebase;

public class AuthentificationControllerTests
{
    private AuthentificationController CreateController(Mock<IFirebaseAuthWrapper>? mockAuth = null)
    {
        return new AuthentificationController(mockAuth?.Object ?? Mock.Of<IFirebaseAuthWrapper>());
    }

    [Fact]
    public async Task VerifyFirebaseToken_ReturnsOkResult_WithCorrectUid()
    {
        var mockAuth = new Mock<IFirebaseAuthWrapper>();
        mockAuth.Setup(auth => auth.VerifyIdTokenAndGetUidAsync("valid-token"))
                .ReturnsAsync("mocked-uid");

        var controller = CreateController(mockAuth);

        var result = await controller.VerifyFirebaseToken("valid-token");

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        dynamic response = okResult.Value!;
        Assert.Equal("mocked-uid", (string)response.uid);
    }

    [Fact]
    public async Task VerifyToken_WithValidDto_ReturnsUid()
    {
        var mockAuth = new Mock<IFirebaseAuthWrapper>();
        mockAuth.Setup(auth => auth.VerifyIdTokenAndGetUidAsync("valid-token"))
                .ReturnsAsync("mocked-uid");

        var controller = CreateController(mockAuth);
        var model = new FirebaseAuthDto { IdToken = "valid-token" };

        var result = await controller.VerifyToken(model);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        dynamic response = okResult.Value!;
        Assert.Equal("mocked-uid", (string)response.uid);
    }

    [Fact]
    public async Task GetUid_WithValidToken_ReturnsUid()
    {
        var mockAuth = new Mock<IFirebaseAuthWrapper>();
        mockAuth.Setup(auth => auth.VerifyIdTokenAndGetUidAsync("valid-token"))
                .ReturnsAsync("mocked-uid");

        var controller = CreateController(mockAuth);

        var result = await controller.GetUid("valid-token");

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        dynamic response = okResult.Value!;
        Assert.Equal("mocked-uid", (string)response.uid);
    }
}
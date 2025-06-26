#nullable enable
using Backend.Services;
using Xunit;
using Backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Backend.Dtos;
using Backend.Services.Firebase;

public class AuthentificationControllerTests
    {
        private AuthentificationController CreateController(
        Mock<IFirebaseAuthWrapper>? mockAuth = null,
        Mock<IUserQueryService>? mockUserQuery = null)
    {
        return new AuthentificationController(
            mockAuth?.Object ?? Mock.Of<IFirebaseAuthWrapper>(),
            mockUserQuery?.Object ?? Mock.Of<IUserQueryService>());
    }

    [Fact]
    public async Task VerifyFirebaseToken_ReturnsOkResult_WithCorrectUid()
    {
        var mockAuth = new Mock<IFirebaseAuthWrapper>();
        mockAuth.Setup(auth => auth.VerifyIdTokenAndGetUidAsync("valid-token"))
                .ReturnsAsync("mocked-uid");

        var controller = CreateController(mockAuth);
        var result = await controller.VerifyFirebaseToken(new FirebaseTokenRequest { IdToken = "valid-token" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var response = Assert.IsType<TokenResponseDto>(okResult.Value);
        Assert.Equal("mocked-uid", response.Uid);
    }

    [Fact]
    public async Task VerifyFirebaseToken_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        var mockAuth = new Mock<IFirebaseAuthWrapper>();
        mockAuth.Setup(auth => auth.VerifyIdTokenAndGetUidAsync("invalid-token"))
                .ThrowsAsync(new Exception("Token invalid"));

        var controller = new AuthentificationController(mockAuth.Object, Mock.Of<IUserQueryService>());
        var result = await controller.VerifyFirebaseToken(new FirebaseTokenRequest { IdToken = "invalid-token" });

        Assert.IsType<UnauthorizedObjectResult>(result);
}
}
using ForeFrontWebApplication.Controllers;
using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ForeFrontWebApplication.Tests.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_authService, NullLogger<AuthController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static LoginRequest ValidRequest() => new()
    {
        Email = "admin@forefront.dev",
        Password = "Admin@1234"
    };

    private static TokenResponse FakeToken() => new()
    {
        Token = "eyJhbGciOiJIUzI1NiJ9.test",
        ExpiresAt = DateTime.UtcNow.AddHours(1)
    };

    [Fact]
    public void Token_ValidCredentials_ReturnsOkWithToken()
    {
        _authService.Authenticate(Arg.Any<string>(), Arg.Any<string>()).Returns(FakeToken());

        var result = _sut.Token(ValidRequest());

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TokenResponse>(ok.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public void Token_InvalidCredentials_ReturnsUnauthorized()
    {
        _authService.Authenticate(Arg.Any<string>(), Arg.Any<string>()).Returns((TokenResponse?)null);

        Assert.IsType<UnauthorizedResult>(_sut.Token(ValidRequest()));
    }

    [Fact]
    public void Token_InvalidModelState_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Email", "Required");

        Assert.IsType<BadRequestObjectResult>(_sut.Token(ValidRequest()));
    }

    [Fact]
    public void Token_InvalidCredentials_DoesNotLeakWhichFieldWasWrong()
    {
        // Both unknown email and wrong password must return the same 401 — no hint to callers
        _authService.Authenticate(Arg.Any<string>(), Arg.Any<string>()).Returns((TokenResponse?)null);

        var result = _sut.Token(ValidRequest());

        // Must not be 403 (which would confirm the user exists) — must be plain 401
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Token_ValidCredentials_ServiceCalledWithExactCredentials()
    {
        var request = ValidRequest();
        _authService.Authenticate(Arg.Any<string>(), Arg.Any<string>()).Returns(FakeToken());

        _sut.Token(request);

        _authService.Received(1).Authenticate(request.Email, request.Password);
    }
}

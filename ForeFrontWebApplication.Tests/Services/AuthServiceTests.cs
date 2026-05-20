using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using NSubstitute;
using Xunit;

namespace ForeFrontWebApplication.Tests.Services;

public class AuthServiceTests
{
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _tokenService
            .GenerateToken(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new TokenResponse { Token = "mock-token", ExpiresAt = DateTime.UtcNow.AddHours(1) });

        _sut = new AuthService(_tokenService);
    }

    [Theory]
    [InlineData("admin@forefront.dev",     "Admin@1234",     "Admin")]
    [InlineData("warehouse@forefront.dev", "Warehouse@1234", "Warehouse")]
    [InlineData("customer@forefront.dev",  "Customer@1234",  "Customer")]
    public void Authenticate_ValidCredentials_ReturnsToken(string email, string password, string role)
    {
        var result = _sut.Authenticate(email, password);

        Assert.NotNull(result);
        Assert.Equal("mock-token", result.Token);
        _tokenService.Received(1).GenerateToken(email, role);
    }

    [Fact]
    public void Authenticate_UnknownEmail_ReturnsNull()
    {
        var result = _sut.Authenticate("unknown@example.com", "Admin@1234");

        Assert.Null(result);
        _tokenService.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void Authenticate_WrongPassword_ReturnsNull()
    {
        var result = _sut.Authenticate("admin@forefront.dev", "wrong-password");

        Assert.Null(result);
        _tokenService.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void Authenticate_EmailIsCaseInsensitive()
    {
        var result = _sut.Authenticate("ADMIN@FOREFRONT.DEV", "Admin@1234");

        Assert.NotNull(result);
    }

    [Fact]
    public void Authenticate_CorrectEmailButEmptyPassword_ReturnsNull()
    {
        var result = _sut.Authenticate("admin@forefront.dev", string.Empty);

        Assert.Null(result);
    }
}

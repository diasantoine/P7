using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class TokenServiceTests
    {
        private static readonly JwtSettings Settings = new JwtSettings
        {
            Key = "unit-tests-signing-key-0123456789abcdef",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };

        private readonly TokenService _service = new TokenService(Options.Create(Settings));

        [Fact]
        public void CreateToken_ContainsUserIdNameAndRoles()
        {
            var user = new User { Id = 7, UserName = "alice" };

            LoginResponse response = _service.CreateToken(user, new[] { Roles.Admin });

            ClaimsPrincipal principal = Validate(response.Token, Settings.GetSigningKey());
            Assert.Equal("alice", principal.Identity!.Name);
            Assert.Equal("7", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.True(principal.IsInRole(Roles.Admin));
            Assert.False(principal.IsInRole(Roles.User));
        }

        [Fact]
        public void CreateToken_ExpiresAfterConfiguredDuration()
        {
            DateTime before = DateTime.UtcNow;

            LoginResponse response = _service.CreateToken(new User { Id = 1, UserName = "bob" }, Array.Empty<string>());

            Assert.InRange(
                response.ExpiresAt,
                before.AddMinutes(Settings.ExpiryMinutes),
                DateTime.UtcNow.AddMinutes(Settings.ExpiryMinutes));
        }

        [Fact]
        public void CreateToken_IsRejectedWithAnotherKey()
        {
            LoginResponse response = _service.CreateToken(new User { Id = 1, UserName = "bob" }, Array.Empty<string>());
            var otherKey = new JwtSettings { Key = "another-signing-key-0123456789abcdefgh" }.GetSigningKey();

            Assert.ThrowsAny<SecurityTokenException>(() => Validate(response.Token, otherKey));
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("0123456789012345678901234567890", false)]   // 31 characters
        [InlineData("01234567890123456789012345678901", true)]   // 32 characters
        public void HasValidKey_RequiresAtLeast32Characters(string key, bool expected)
        {
            var settings = new JwtSettings { Key = key };

            Assert.Equal(expected, settings.HasValidKey());
        }

        private static ClaimsPrincipal Validate(string token, SecurityKey key)
        {
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = Settings.Issuer,
                ValidAudience = Settings.Audience,
                IssuerSigningKey = key
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
        }
    }
}

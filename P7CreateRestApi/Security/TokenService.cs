using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dot.Net.WebApi.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Security
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public LoginResponse CreateToken(User user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var credentials = new SigningCredentials(_settings.GetSigningKey(), SecurityAlgorithms.HmacSha256);
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}

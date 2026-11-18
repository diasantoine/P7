using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Dot.Net.WebApi.Security
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";
        public const int MinimumKeyLength = 32;

        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;

        public bool HasValidKey()
        {
            return !string.IsNullOrWhiteSpace(Key)
                && Encoding.UTF8.GetByteCount(Key) >= MinimumKeyLength;
        }

        public SymmetricSecurityKey GetSigningKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        }
    }
}

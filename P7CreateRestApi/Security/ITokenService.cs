using Dot.Net.WebApi.Domain;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Security
{
    public interface ITokenService
    {
        LoginResponse CreateToken(User user, IEnumerable<string> roles);
    }
}

using Microsoft.AspNetCore.Identity;

namespace Dot.Net.WebApi.Domain
{
    // Id, UserName and the hashed password come from IdentityUser; roles live in the Identity role tables.
    public class User : IdentityUser<int>
    {
        public string? Fullname { get; set; }
    }
}

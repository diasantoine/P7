using System.ComponentModel.DataAnnotations;
using Dot.Net.WebApi.Security;

namespace P7CreateRestApi.Models
{
    // What the client sends to create or update a user. The password only travels in this direction.
    public class UserInputDto
    {
        public const string RolePattern = "^(" + Roles.Admin + "|" + Roles.User + ")$";

        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire.")]
        public string Username { get; set; } = string.Empty;

        // The strength rules (length, uppercase, digit, symbol) are checked by Identity.
        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom complet est obligatoire.")]
        public string Fullname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [RegularExpression(RolePattern, ErrorMessage = "Le rôle doit être Admin ou User.")]
        public string Role { get; set; } = string.Empty;
    }
}

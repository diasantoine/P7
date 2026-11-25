using Microsoft.AspNetCore.Identity;

namespace Dot.Net.WebApi.Security
{
    // Identity writes its error messages in English: these are the ones the API can return, in French.
    // The codes stay the same, only the descriptions change.
    public class FrenchIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordTooShort(int length)
        {
            return Error(nameof(PasswordTooShort), $"Le mot de passe doit contenir au moins {length} caractères.");
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return Error(nameof(PasswordRequiresUpper), "Le mot de passe doit contenir au moins une majuscule.");
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return Error(nameof(PasswordRequiresDigit), "Le mot de passe doit contenir au moins un chiffre.");
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return Error(nameof(PasswordRequiresNonAlphanumeric), "Le mot de passe doit contenir au moins un symbole.");
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return Error(nameof(DuplicateUserName), $"Le nom d'utilisateur '{userName}' est déjà pris.");
        }

        public override IdentityError InvalidUserName(string userName)
        {
            return Error(nameof(InvalidUserName), $"Le nom d'utilisateur '{userName}' est invalide : lettres, chiffres et -._@+ uniquement.");
        }

        private static IdentityError Error(string code, string description)
        {
            return new IdentityError { Code = code, Description = description };
        }
    }
}

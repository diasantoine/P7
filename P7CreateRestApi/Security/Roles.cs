namespace Dot.Net.WebApi.Security
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";

        public static readonly string[] All = { Admin, User };
    }
}

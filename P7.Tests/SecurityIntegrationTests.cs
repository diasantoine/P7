using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class SecurityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private const string TraderPassword = "Trader#12345";

        private readonly CustomWebApplicationFactory _factory;

        public SecurityIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Endpoint_WithoutToken_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/Home");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Endpoint_WithInvalidToken_Returns401()
        {
            HttpClient client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

            HttpResponseMessage response = await client.GetAsync("/Home");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsUsableToken()
        {
            HttpClient client = _factory.CreateClient();

            string token = await LoginAsync(client, CustomWebApplicationFactory.AdminUserName, CustomWebApplicationFactory.AdminPassword);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.GetAsync("/Home");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/login",
                new LoginModel { Username = CustomWebApplicationFactory.AdminUserName, Password = "Wrong#12345" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithUnknownUser_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/login",
                new LoginModel { Username = "nobody", Password = CustomWebApplicationFactory.AdminPassword });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithMissingFields_Returns400()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/login",
                new LoginModel { Username = string.Empty, Password = string.Empty });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AdminEndpoint_WithAdminRole_Returns200()
        {
            HttpClient client = _factory.CreateClient();
            string token = await LoginAsync(client, CustomWebApplicationFactory.AdminUserName, CustomWebApplicationFactory.AdminPassword);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync("/Home/Admin");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AdminEndpoint_WithUserRole_Returns403()
        {
            string userName = await CreateTraderAsync();
            HttpClient client = _factory.CreateClient();
            string token = await LoginAsync(client, userName, TraderPassword);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage adminResponse = await client.GetAsync("/Home/Admin");
            HttpResponseMessage homeResponse = await client.GetAsync("/Home");

            Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, homeResponse.StatusCode);
        }

        [Theory]
        [InlineData("Abcde1!", "PasswordTooShort")]
        [InlineData("abcdefg1!", "PasswordRequiresUpper")]
        [InlineData("Abcdefgh!", "PasswordRequiresDigit")]
        [InlineData("Abcdefg12", "PasswordRequiresNonAlphanumeric")]
        public async Task CreateUser_WithWeakPassword_IsRejected(string password, string expectedError)
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            IdentityResult result = await userManager.CreateAsync(NewUser(), password);

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, error => error.Code == expectedError);
        }

        [Fact]
        public async Task CreateUser_WithEightCharacterStrongPassword_IsAccepted()
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            IdentityResult result = await userManager.CreateAsync(NewUser(), "Abcdef1!");

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task CreateUser_StoresHashedPassword()
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            User user = NewUser();

            await userManager.CreateAsync(user, TraderPassword);

            Assert.False(string.IsNullOrEmpty(user.PasswordHash));
            Assert.NotEqual(TraderPassword, user.PasswordHash);
        }

        private static User NewUser()
        {
            return new User { UserName = "user-" + Guid.NewGuid().ToString("N") };
        }

        private async Task<string> CreateTraderAsync()
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            User user = NewUser();

            Assert.True((await userManager.CreateAsync(user, TraderPassword)).Succeeded);
            Assert.True((await userManager.AddToRoleAsync(user, Roles.User)).Succeeded);

            return user.UserName;
        }

        private static async Task<string> LoginAsync(HttpClient client, string userName, string password)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/login",
                new LoginModel { Username = userName, Password = password });
            response.EnsureSuccessStatusCode();

            LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>();

            return body!.Token;
        }
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class UserIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private const string Password = "Trader#12345";
        private const string NewPassword = "Changed#6789";

        private readonly CustomWebApplicationFactory _factory;

        public UserIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/users");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task EveryRoute_WithUserRole_Returns403()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();
            UserInputDto trader = NewInput();
            await CreateAsync(admin, trader);
            HttpClient client = await LoginAsync(trader.Username, trader.Password);

            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/users")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/users/1")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/users", NewInput())).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync("/users/1", NewInput())).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync("/users/1")).StatusCode);
        }

        [Fact]
        public async Task Create_Read_Update_Delete_WorkEndToEnd()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();
            UserInputDto input = NewInput();

            // Create: the answer is the up-to-date list, without any password.
            HttpResponseMessage createResponse = await admin.PostAsJsonAsync("/users", input);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            string createBody = await createResponse.Content.ReadAsStringAsync();
            Assert.DoesNotContain("password", createBody, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(input.Password, createBody);
            List<UserDto>? afterCreate = await createResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            UserDto created = Assert.Single(afterCreate!, user => user.Username == input.Username);
            Assert.True(created.Id > 0);
            Assert.Equal("User", created.Role);

            // The new account can log in.
            await LoginAsync(input.Username, input.Password);

            // Read one.
            UserDto? read = await admin.GetFromJsonAsync<UserDto>($"/users/{created.Id}");
            Assert.Equal(input.Fullname, read!.Fullname);

            // Update: full name, role and password change.
            var update = new UserInputDto
            {
                Username = input.Username,
                Password = NewPassword,
                Fullname = "Renamed Trader",
                Role = "Admin"
            };
            HttpResponseMessage updateResponse = await admin.PutAsJsonAsync($"/users/{created.Id}", update);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            List<UserDto>? afterUpdate = await updateResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            UserDto updated = Assert.Single(afterUpdate!, user => user.Id == created.Id);
            Assert.Equal("Renamed Trader", updated.Fullname);
            Assert.Equal("Admin", updated.Role);

            // The old password no longer works, the new one does and carries the new role.
            Assert.Equal(HttpStatusCode.Unauthorized, (await PostLoginAsync(input.Username, input.Password)).StatusCode);
            HttpClient promoted = await LoginAsync(input.Username, NewPassword);
            Assert.Equal(HttpStatusCode.OK, (await promoted.GetAsync("/users")).StatusCode);

            // Delete.
            HttpResponseMessage deleteResponse = await admin.DeleteAsync($"/users/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            List<UserDto>? afterDelete = await deleteResponse.Content.ReadFromJsonAsync<List<UserDto>>();
            Assert.DoesNotContain(afterDelete!, user => user.Id == created.Id);

            Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/users/{created.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await PostLoginAsync(input.Username, NewPassword)).StatusCode);
        }

        [Theory]
        [InlineData("Abcde1!", "Le mot de passe doit contenir au moins 8 caractères.")]
        [InlineData("abcdefg1!", "Le mot de passe doit contenir au moins une majuscule.")]
        [InlineData("Abcdefgh!", "Le mot de passe doit contenir au moins un chiffre.")]
        [InlineData("Abcdefg12", "Le mot de passe doit contenir au moins un symbole.")]
        public async Task Create_WithWeakPassword_Returns400WithMessage(string password, string expectedMessage)
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();
            UserInputDto input = NewInput();
            input.Password = password;

            HttpResponseMessage response = await admin.PostAsJsonAsync("/users", input);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            List<string>? messages = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.Contains(expectedMessage, messages!);
        }

        [Fact]
        public async Task Create_WithUserNameAlreadyTaken_Returns400WithMessage()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();
            UserInputDto input = NewInput();
            await CreateAsync(admin, input);

            HttpResponseMessage response = await admin.PostAsJsonAsync("/users", input);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            List<string>? messages = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.Contains($"Le nom d'utilisateur '{input.Username}' est déjà pris.", messages!);
        }

        [Fact]
        public async Task Create_WithForbiddenCharacterInUserName_Returns400WithMessage()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();
            UserInputDto input = NewInput();
            input.Username = "bob trader";

            HttpResponseMessage response = await admin.PostAsJsonAsync("/users", input);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            List<string>? messages = await response.Content.ReadFromJsonAsync<List<string>>();
            Assert.Contains(messages!, message => message.StartsWith("Le nom d'utilisateur 'bob trader' est invalide"));
        }

        [Fact]
        public async Task Create_WithUnknownRole_Returns400WithMessage()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();
            UserInputDto input = NewInput();
            input.Role = "SuperAdmin";

            HttpResponseMessage response = await admin.PostAsJsonAsync("/users", input);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Admin ou User", await ReadDecodedAsync(response));
        }

        [Fact]
        public async Task Create_WithMissingFields_Returns400WithMessages()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await admin.PostAsJsonAsync("/users", new UserInputDto());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await ReadDecodedAsync(response);
            Assert.Contains("Le nom d'utilisateur est obligatoire.", body);
            Assert.Contains("Le mot de passe est obligatoire.", body);
            Assert.Contains("Le nom complet est obligatoire.", body);
            Assert.Contains("Le rôle est obligatoire.", body);
        }

        [Fact]
        public async Task Update_WithWeakPassword_Returns400AndKeepsTheAccountUnchanged()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();
            UserInputDto input = NewInput();
            UserDto created = await CreateAsync(admin, input);
            var update = new UserInputDto
            {
                Username = input.Username,
                Password = "weak",
                Fullname = "Should not be saved",
                Role = "Admin"
            };

            HttpResponseMessage response = await admin.PutAsJsonAsync($"/users/{created.Id}", update);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            UserDto? read = await admin.GetFromJsonAsync<UserDto>($"/users/{created.Id}");
            Assert.Equal(input.Fullname, read!.Fullname);
            Assert.Equal("User", read.Role);
            await LoginAsync(input.Username, input.Password);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await admin.PutAsJsonAsync("/users/999999", NewInput());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithUnknownId_Returns404()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await admin.DeleteAsync("/users/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_OwnAccount_Returns400AndKeepsTheAccount()
        {
            HttpClient admin = await _factory.CreateAdminClientAsync();
            List<UserDto>? users = await admin.GetFromJsonAsync<List<UserDto>>("/users");
            UserDto self = Assert.Single(users!, user => user.Username == CustomWebApplicationFactory.AdminUserName);

            HttpResponseMessage response = await admin.DeleteAsync($"/users/{self.Id}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/users/{self.Id}")).StatusCode);
        }

        private static UserInputDto NewInput()
        {
            return new UserInputDto
            {
                Username = "user-" + Guid.NewGuid().ToString("N"),
                Password = Password,
                Fullname = "Test Trader",
                Role = "User"
            };
        }

        private static async Task<UserDto> CreateAsync(HttpClient admin, UserInputDto input)
        {
            HttpResponseMessage response = await admin.PostAsJsonAsync("/users", input);
            response.EnsureSuccessStatusCode();
            List<UserDto>? users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

            return users!.Single(user => user.Username == input.Username);
        }

        // The validation answer escapes the accents (ô): decode it before comparing with the French messages.
        private static async Task<string> ReadDecodedAsync(HttpResponseMessage response)
        {
            return System.Text.RegularExpressions.Regex.Unescape(await response.Content.ReadAsStringAsync());
        }

        private Task<HttpResponseMessage> PostLoginAsync(string userName, string password)
        {
            return _factory.CreateClient().PostAsJsonAsync(
                "/login",
                new LoginModel { Username = userName, Password = password });
        }

        private async Task<HttpClient> LoginAsync(string userName, string password)
        {
            HttpResponseMessage response = await PostLoginAsync(userName, password);
            response.EnsureSuccessStatusCode();
            LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>();

            HttpClient client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);

            return client;
        }
    }
}

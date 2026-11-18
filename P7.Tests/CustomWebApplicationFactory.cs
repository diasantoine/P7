using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dot.Net.WebApi.Data;
using P7CreateRestApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace P7.Tests
{
    // Starts the real application in memory: same pipeline and security, but an in-memory database
    // and test-only secrets instead of SQL Server and user secrets.
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public const string AdminUserName = "admin";
        public const string AdminPassword = "Admin#12345";

        private readonly string _databaseName = "P7Tests-" + Guid.NewGuid();

        public async Task<HttpClient> CreateAdminClientAsync()
        {
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/login",
                new LoginModel { Username = AdminUserName, Password = AdminPassword });
            response.EnsureSuccessStatusCode();

            LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);

            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = "integration-tests-signing-key-0123456789",
                    ["Jwt:Issuer"] = "P7.Tests",
                    ["Jwt:Audience"] = "P7.Tests",
                    ["Seed:AdminUserName"] = AdminUserName,
                    ["Seed:AdminPassword"] = AdminPassword
                });
            });

            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? sqlServerOptions = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(DbContextOptions<LocalDbContext>));

                if (sqlServerOptions != null)
                {
                    services.Remove(sqlServerOptions);
                }

                services.AddDbContext<LocalDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        }
    }
}

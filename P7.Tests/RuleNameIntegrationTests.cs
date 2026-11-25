using System.Net;
using System.Net.Http.Json;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class RuleNameIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public RuleNameIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/rulenames");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_Read_Update_Delete_WorkEndToEnd()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();
            string name = "Rule-" + Guid.NewGuid().ToString("N");

            // Create: the answer is the up-to-date list.
            HttpResponseMessage createResponse = await client.PostAsJsonAsync(
                "/rulenames",
                new RuleNameDto
                {
                    Name = name,
                    Description = "Description A",
                    Json = "{\"a\":1}",
                    Template = "Template A",
                    SqlStr = "SELECT 1",
                    SqlPart = "WHERE 1 = 1"
                });
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            List<RuleNameDto>? afterCreate = await createResponse.Content.ReadFromJsonAsync<List<RuleNameDto>>();
            RuleNameDto created = Assert.Single(afterCreate!, ruleName => ruleName.Name == name);
            Assert.True(created.Id > 0);

            // Read one.
            RuleNameDto? read = await client.GetFromJsonAsync<RuleNameDto>($"/rulenames/{created.Id}");
            Assert.Equal("Description A", read!.Description);
            Assert.Equal("{\"a\":1}", read.Json);
            Assert.Equal("Template A", read.Template);
            Assert.Equal("SELECT 1", read.SqlStr);
            Assert.Equal("WHERE 1 = 1", read.SqlPart);

            // Update: the optional fields left out are cleared.
            HttpResponseMessage updateResponse = await client.PutAsJsonAsync(
                $"/rulenames/{created.Id}",
                new RuleNameDto { Name = name, Description = "Description B" });
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            List<RuleNameDto>? afterUpdate = await updateResponse.Content.ReadFromJsonAsync<List<RuleNameDto>>();
            RuleNameDto updated = Assert.Single(afterUpdate!, ruleName => ruleName.Id == created.Id);
            Assert.Equal("Description B", updated.Description);
            Assert.Null(updated.SqlStr);

            // Delete.
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"/rulenames/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            List<RuleNameDto>? afterDelete = await deleteResponse.Content.ReadFromJsonAsync<List<RuleNameDto>>();
            Assert.DoesNotContain(afterDelete!, ruleName => ruleName.Id == created.Id);

            HttpResponseMessage readDeleted = await client.GetAsync($"/rulenames/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, readDeleted.StatusCode);
        }

        [Fact]
        public async Task Create_WithNameOnly_IsAccepted()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/rulenames",
                new RuleNameDto { Name = "Rule-" + Guid.NewGuid().ToString("N") });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutName_Returns400WithMessage()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/rulenames",
                new RuleNameDto { Name = string.Empty, Description = "Description A" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Le nom de la règle est obligatoire.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PutAsJsonAsync("/rulenames/999999", new RuleNameDto { Name = "Rule A" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.DeleteAsync("/rulenames/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}

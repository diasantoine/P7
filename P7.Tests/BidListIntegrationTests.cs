using System.Net;
using System.Net.Http.Json;
using System.Text;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class BidListIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public BidListIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/bidlists");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_Read_Update_Delete_WorkEndToEnd()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();
            string account = "Account-" + Guid.NewGuid().ToString("N");

            // Create: the answer is the up-to-date list.
            HttpResponseMessage createResponse = await client.PostAsJsonAsync(
                "/bidlists",
                new BidListDto { Account = account, BidType = "Type A", BidQuantity = 10 });
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            List<BidListDto>? afterCreate = await createResponse.Content.ReadFromJsonAsync<List<BidListDto>>();
            BidListDto created = Assert.Single(afterCreate!, bidList => bidList.Account == account);
            Assert.True(created.BidListId > 0);

            // Read one.
            BidListDto? read = await client.GetFromJsonAsync<BidListDto>($"/bidlists/{created.BidListId}");
            Assert.Equal("Type A", read!.BidType);
            Assert.Equal(10, read.BidQuantity);

            // Update.
            HttpResponseMessage updateResponse = await client.PutAsJsonAsync(
                $"/bidlists/{created.BidListId}",
                new BidListDto { Account = account, BidType = "Type B", BidQuantity = 25 });
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            List<BidListDto>? afterUpdate = await updateResponse.Content.ReadFromJsonAsync<List<BidListDto>>();
            BidListDto updated = Assert.Single(afterUpdate!, bidList => bidList.BidListId == created.BidListId);
            Assert.Equal("Type B", updated.BidType);
            Assert.Equal(25, updated.BidQuantity);

            // Delete.
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"/bidlists/{created.BidListId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            List<BidListDto>? afterDelete = await deleteResponse.Content.ReadFromJsonAsync<List<BidListDto>>();
            Assert.DoesNotContain(afterDelete!, bidList => bidList.BidListId == created.BidListId);

            HttpResponseMessage readDeleted = await client.GetAsync($"/bidlists/{created.BidListId}");
            Assert.Equal(HttpStatusCode.NotFound, readDeleted.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutAccountAndType_Returns400WithMessages()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/bidlists",
                new BidListDto { Account = string.Empty, BidType = string.Empty, BidQuantity = 10 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Le compte est obligatoire.", body);
            Assert.Contains("Le type de l'offre est obligatoire.", body);
        }

        [Theory]
        [InlineData("\"abc\"")]
        [InlineData("-5")]
        public async Task Create_WithInvalidQuantity_Returns400(string quantity)
        {
            HttpClient client = await _factory.CreateAdminClientAsync();
            var content = new StringContent(
                $"{{\"account\":\"Account A\",\"bidType\":\"Type A\",\"bidQuantity\":{quantity}}}",
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await client.PostAsync("/bidlists", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PutAsJsonAsync(
                "/bidlists/999999",
                new BidListDto { Account = "Account A", BidType = "Type A", BidQuantity = 10 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.DeleteAsync("/bidlists/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}

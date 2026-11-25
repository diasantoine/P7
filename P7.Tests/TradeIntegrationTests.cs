using System.Net;
using System.Net.Http.Json;
using System.Text;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class TradeIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public TradeIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/trades");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_Read_Update_Delete_WorkEndToEnd()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();
            string account = "Account-" + Guid.NewGuid().ToString("N");

            // Create: the answer is the up-to-date list.
            HttpResponseMessage createResponse = await client.PostAsJsonAsync(
                "/trades",
                new TradeDto { Account = account, AccountType = "Type A", BuyQuantity = 10 });
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            List<TradeDto>? afterCreate = await createResponse.Content.ReadFromJsonAsync<List<TradeDto>>();
            TradeDto created = Assert.Single(afterCreate!, trade => trade.Account == account);
            Assert.True(created.TradeId > 0);

            // Read one.
            TradeDto? read = await client.GetFromJsonAsync<TradeDto>($"/trades/{created.TradeId}");
            Assert.Equal("Type A", read!.AccountType);
            Assert.Equal(10, read.BuyQuantity);

            // Update.
            HttpResponseMessage updateResponse = await client.PutAsJsonAsync(
                $"/trades/{created.TradeId}",
                new TradeDto { Account = account, AccountType = "Type B", BuyQuantity = 25 });
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            List<TradeDto>? afterUpdate = await updateResponse.Content.ReadFromJsonAsync<List<TradeDto>>();
            TradeDto updated = Assert.Single(afterUpdate!, trade => trade.TradeId == created.TradeId);
            Assert.Equal("Type B", updated.AccountType);
            Assert.Equal(25, updated.BuyQuantity);

            // Delete.
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"/trades/{created.TradeId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            List<TradeDto>? afterDelete = await deleteResponse.Content.ReadFromJsonAsync<List<TradeDto>>();
            Assert.DoesNotContain(afterDelete!, trade => trade.TradeId == created.TradeId);

            HttpResponseMessage readDeleted = await client.GetAsync($"/trades/{created.TradeId}");
            Assert.Equal(HttpStatusCode.NotFound, readDeleted.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutAccountAndType_Returns400WithMessages()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/trades",
                new TradeDto { Account = string.Empty, AccountType = string.Empty, BuyQuantity = 10 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Le compte est obligatoire.", body);
            Assert.Contains("Le type de compte est obligatoire.", body);
        }

        [Theory]
        [InlineData("\"abc\"")]
        [InlineData("-5")]
        public async Task Create_WithInvalidQuantity_Returns400(string quantity)
        {
            HttpClient client = await _factory.CreateAdminClientAsync();
            var content = new StringContent(
                $"{{\"account\":\"Account A\",\"accountType\":\"Type A\",\"buyQuantity\":{quantity}}}",
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await client.PostAsync("/trades", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PutAsJsonAsync(
                "/trades/999999",
                new TradeDto { Account = "Account A", AccountType = "Type A", BuyQuantity = 10 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.DeleteAsync("/trades/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}

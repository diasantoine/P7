using System.Net;
using System.Net.Http.Json;
using System.Text;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class RatingIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public RatingIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/ratings");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_Read_Update_Delete_WorkEndToEnd()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();
            string moodys = "Moodys-" + Guid.NewGuid().ToString("N");

            // Create: the answer is the up-to-date list.
            HttpResponseMessage createResponse = await client.PostAsJsonAsync(
                "/ratings",
                new RatingDto { MoodysRating = moodys, SandPRating = "AAA", FitchRating = "AA+", OrderNumber = 1 });
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            List<RatingDto>? afterCreate = await createResponse.Content.ReadFromJsonAsync<List<RatingDto>>();
            RatingDto created = Assert.Single(afterCreate!, rating => rating.MoodysRating == moodys);
            Assert.True(created.Id > 0);

            // Read one.
            RatingDto? read = await client.GetFromJsonAsync<RatingDto>($"/ratings/{created.Id}");
            Assert.Equal("AAA", read!.SandPRating);
            Assert.Equal("AA+", read.FitchRating);
            Assert.Equal((byte)1, read.OrderNumber);

            // Update.
            HttpResponseMessage updateResponse = await client.PutAsJsonAsync(
                $"/ratings/{created.Id}",
                new RatingDto { MoodysRating = moodys, SandPRating = "BBB", FitchRating = "BB-", OrderNumber = 7 });
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            List<RatingDto>? afterUpdate = await updateResponse.Content.ReadFromJsonAsync<List<RatingDto>>();
            RatingDto updated = Assert.Single(afterUpdate!, rating => rating.Id == created.Id);
            Assert.Equal("BBB", updated.SandPRating);
            Assert.Equal("BB-", updated.FitchRating);
            Assert.Equal((byte)7, updated.OrderNumber);

            // Delete.
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"/ratings/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            List<RatingDto>? afterDelete = await deleteResponse.Content.ReadFromJsonAsync<List<RatingDto>>();
            Assert.DoesNotContain(afterDelete!, rating => rating.Id == created.Id);

            HttpResponseMessage readDeleted = await client.GetAsync($"/ratings/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, readDeleted.StatusCode);
        }

        [Fact]
        public async Task Create_WithEmptyBody_Returns400WithEveryMessage()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PostAsJsonAsync("/ratings", new RatingDto());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("La notation Moody's est obligatoire.", body);
            Assert.Contains("La notation S&P est obligatoire.", body);
            Assert.Contains("La notation Fitch est obligatoire.", body);
            Assert.Contains("Le numéro d'ordre est obligatoire.", body);
        }

        [Theory]
        [InlineData("\"abc\"")]   // not a number
        [InlineData("300")]       // above the byte range
        [InlineData("-1")]        // below the byte range
        [InlineData("1.5")]       // not a whole number
        public async Task Create_WithInvalidOrderNumber_Returns400(string orderNumber)
        {
            HttpClient client = await _factory.CreateAdminClientAsync();
            var content = new StringContent(
                $"{{\"moodysRating\":\"Aaa\",\"sandPRating\":\"AAA\",\"fitchRating\":\"AA+\",\"orderNumber\":{orderNumber}}}",
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await client.PostAsync("/ratings", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PutAsJsonAsync(
                "/ratings/999999",
                new RatingDto { MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AA+", OrderNumber = 1 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.DeleteAsync("/ratings/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}

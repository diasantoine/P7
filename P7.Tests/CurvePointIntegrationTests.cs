using System.Net;
using System.Net.Http.Json;
using System.Text;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class CurvePointIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public CurvePointIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/curvepoints");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_Read_Update_Delete_WorkEndToEnd()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            // Create: the answer is the up-to-date list.
            HttpResponseMessage createResponse = await client.PostAsJsonAsync(
                "/curvepoints",
                new CurvePointDto { CurveId = 12, Term = 14.5, CurvePointValue = 10.25 });
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            List<CurvePointDto>? afterCreate = await createResponse.Content.ReadFromJsonAsync<List<CurvePointDto>>();
            CurvePointDto created = Assert.Single(afterCreate!, curvePoint => curvePoint.Term == 14.5);
            Assert.True(created.Id > 0);

            // Read one.
            CurvePointDto? read = await client.GetFromJsonAsync<CurvePointDto>($"/curvepoints/{created.Id}");
            Assert.Equal((byte)12, read!.CurveId);
            Assert.Equal(10.25, read.CurvePointValue);

            // Update.
            HttpResponseMessage updateResponse = await client.PutAsJsonAsync(
                $"/curvepoints/{created.Id}",
                new CurvePointDto { CurveId = 13, Term = 20, CurvePointValue = -1.5 });
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            List<CurvePointDto>? afterUpdate = await updateResponse.Content.ReadFromJsonAsync<List<CurvePointDto>>();
            CurvePointDto updated = Assert.Single(afterUpdate!, curvePoint => curvePoint.Id == created.Id);
            Assert.Equal((byte)13, updated.CurveId);
            Assert.Equal(20, updated.Term);
            Assert.Equal(-1.5, updated.CurvePointValue);

            // Delete.
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"/curvepoints/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            List<CurvePointDto>? afterDelete = await deleteResponse.Content.ReadFromJsonAsync<List<CurvePointDto>>();
            Assert.DoesNotContain(afterDelete!, curvePoint => curvePoint.Id == created.Id);

            HttpResponseMessage readDeleted = await client.GetAsync($"/curvepoints/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, readDeleted.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutCurveId_Returns400WithMessage()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/curvepoints",
                new CurvePointDto { CurveId = null, Term = 14, CurvePointValue = 10 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Ne doit pas être vide.", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("{\"curveId\":\"abc\",\"term\":14,\"curvePointValue\":10}")]   // not a number
        [InlineData("{\"curveId\":300,\"term\":14,\"curvePointValue\":10}")]       // above the byte range
        [InlineData("{\"curveId\":12,\"term\":-1,\"curvePointValue\":10}")]        // negative term
        [InlineData("{\"curveId\":12,\"term\":14,\"curvePointValue\":\"abc\"}")]   // not a number
        public async Task Create_WithInvalidNumbers_Returns400(string json)
        {
            HttpClient client = await _factory.CreateAdminClientAsync();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/curvepoints", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.PutAsJsonAsync(
                "/curvepoints/999999",
                new CurvePointDto { CurveId = 12, Term = 14, CurvePointValue = 10 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithUnknownId_Returns404()
        {
            HttpClient client = await _factory.CreateAdminClientAsync();

            HttpResponseMessage response = await client.DeleteAsync("/curvepoints/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}

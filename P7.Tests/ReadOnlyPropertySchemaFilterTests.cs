using Dot.Net.WebApi.Swagger;
using Microsoft.OpenApi.Models;
using P7CreateRestApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace P7.Tests
{
    public class ReadOnlyPropertySchemaFilterTests
    {
        [Theory]
        [InlineData(typeof(BidListDto), "bidListId", "account")]
        [InlineData(typeof(CurvePointDto), "id", "curveId")]
        [InlineData(typeof(RatingDto), "id", "moodysRating")]
        [InlineData(typeof(RuleNameDto), "id", "name")]
        public void Apply_MarksOnlyTheIdAsReadOnly(Type dtoType, string idName, string otherName)
        {
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    [idName] = new OpenApiSchema(),
                    [otherName] = new OpenApiSchema()
                }
            };

            new ReadOnlyPropertySchemaFilter().Apply(schema, new SchemaFilterContext(dtoType, null!, null!));

            Assert.True(schema.Properties[idName].ReadOnly);
            Assert.False(schema.Properties[otherName].ReadOnly);
        }

        [Fact]
        public void Apply_WithoutMatchingSchemaProperty_ChangesNothing()
        {
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema> { ["name"] = new OpenApiSchema() }
            };

            new ReadOnlyPropertySchemaFilter().Apply(schema, new SchemaFilterContext(typeof(RuleNameDto), null!, null!));

            Assert.False(schema.Properties["name"].ReadOnly);
        }
    }
}

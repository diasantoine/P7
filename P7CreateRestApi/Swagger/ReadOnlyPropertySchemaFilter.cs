using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dot.Net.WebApi.Swagger
{
    // Marks the properties tagged [ReadOnly(true)] as read-only in the Swagger document:
    // they stay in the responses but disappear from the request body examples.
    public class ReadOnlyPropertySchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            foreach (PropertyInfo property in context.Type.GetProperties())
            {
                if (property.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly != true)
                {
                    continue;
                }

                string jsonName = JsonNamingPolicy.CamelCase.ConvertName(property.Name);

                if (schema.Properties.TryGetValue(jsonName, out OpenApiSchema? propertySchema))
                {
                    propertySchema.ReadOnly = true;
                }
            }
        }
    }
}

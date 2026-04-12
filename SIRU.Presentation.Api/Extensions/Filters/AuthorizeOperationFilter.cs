using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SIRU.Presentation.Api.Extensions.Filters
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authorizeAttributes = context
                .MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToList();

            if (!authorizeAttributes.Any())
            {
                authorizeAttributes = context
                .MethodInfo
                .DeclaringType!
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToList();
            }

            if (!authorizeAttributes.Any()) return;

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                }
            };

            var roles = authorizeAttributes
                .Where(a => a.Roles != null)
                .SelectMany(a => a.Roles!.Split(',').Select(r => r.Trim()))
                .Distinct();

            if (roles.Any())
            {
                operation.Description += $"<br/><b>🔑 Roles permitidos:</b> {string.Join(", ", roles)}";
            }
        }
    }
}

namespace SIRU.Presentation.Api.Extensions
{
    public static class ApiExtension
    {
        public static void UseSwaggerExtension(this IApplicationBuilder app, IEndpointRouteBuilder routeBuilder)
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                var versionDescriptions = routeBuilder.DescribeApiVersions();
                if (versionDescriptions != null && versionDescriptions.Any())
                {
                    foreach (var apiVersion in versionDescriptions)
                    {
                        var url = $"/swagger/{apiVersion.GroupName}/swagger.json";
                        var name = $"Carrito Api - {apiVersion.GroupName.ToUpper()}";
                        opt.SwaggerEndpoint(url, name);
                    }
                }
            });
        }
    }
}

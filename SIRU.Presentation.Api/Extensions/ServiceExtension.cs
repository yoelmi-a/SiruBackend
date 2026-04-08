using Asp.Versioning;
using Microsoft.OpenApi;
using SIRU.Presentation.Api.Extensions.Filters;

namespace SIRU.Presentation.Api.Extensions
{
    public static class ServiceExtension
    {
        public static void AddSwaggerExtension(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                xmlFiles.ForEach(xmlFile => options.IncludeXmlComments(xmlFile));

                options.UseInlineDefinitionsForEnums();

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1.0",
                    Title = "Carrito Api",
                    Description = "Una api para el manejo de rentas de carros de golf.",
                    Contact = new OpenApiContact
                    {
                        Name = "Yoelmi Alcalá",
                        Email = "alcalayoelmi.a@gmail.com",
                        Url = new Uri("https://itla.edu.do")
                    }
                });

                options.DescribeAllParametersInCamelCase();
                options.EnableAnnotations();
                options.OperationFilter<AuthorizeOperationFilter>();

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Introduce tu Bearer token en este formato: 'Bearer { tu token aquí }'"
                });
            });
        }

        public static void AddApiVersioningExtension(this IServiceCollection services)
        {
            services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader());
            }).AddApiExplorer(opt =>
            {
                opt.GroupNameFormat = "'v'VVV";
                opt.SubstituteApiVersionInUrl = true;
            });
        }
    }
}

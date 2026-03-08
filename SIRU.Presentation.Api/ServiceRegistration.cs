//using Microsoft.OpenApi.Models;

namespace SIRU.Presentation.Api
{
    public static class ServiceRegistration
    {
        public static void AddApiLayer(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                //c.SwaggerDoc("v1", new OpenApiInfo
                //{
                //    Version = "v1",
                //    Title = "SIRU API",
                //    Description = "Sistema de Información de Recursos Humanos",
                //    Contact = new OpenApiContact
                //    {
                //        Name = "Equipo de Desarrollo SIRU",
                //        Email = "contacto@siru.com"
                //    }
                //});

                // Incluir documentación XML si se desea (opcional)
            });
        }
    }
}

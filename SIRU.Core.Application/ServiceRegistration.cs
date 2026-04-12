using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Users;
using SIRU.Core.Application.Services.Users;

namespace SIRU.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddScoped<IVacantService, VacantService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<ICandidateService, CandidateService>();
            services.AddScoped<IUserService, UserService>();
        }
    }
}

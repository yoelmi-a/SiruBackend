using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Candidates;
using SIRU.Core.Application.Interfaces.Departments;
using SIRU.Core.Application.Interfaces.Positions;
using SIRU.Core.Application.Interfaces.Vacant;
using SIRU.Core.Application.Services.Candidates;
using SIRU.Core.Application.Services.Departments;
using SIRU.Core.Application.Services.Positions;
using SIRU.Core.Application.Services.Vacant;
using System.Reflection;

namespace SIRU.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddScoped<IVacantService, VacantService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<ICandidateService, CandidateService>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Vacant;
using SIRU.Core.Application.Services.Vacant;
using SIRU.Core.Application.Interfaces.Candidate;
using SIRU.Core.Application.Services.Candidate;
using System.Reflection;

namespace SIRU.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddScoped<IVacantService, VacantService>();
            services.AddScoped<ICandidateService, CandidateService>();
        }
    }
}

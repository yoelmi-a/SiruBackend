using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Candidates;
using SIRU.Core.Application.Interfaces.Departments;
using SIRU.Core.Application.Interfaces.Employees;
using SIRU.Core.Application.Interfaces.Evaluations;
using SIRU.Core.Application.Interfaces.Positions;
using SIRU.Core.Application.Interfaces.Vacants;
using SIRU.Core.Application.Mappings;
using SIRU.Core.Application.Services.Candidates;
using SIRU.Core.Application.Services.Departments;
using SIRU.Core.Application.Services.Employees;
using SIRU.Core.Application.Services.Evaluations;
using SIRU.Core.Application.Services.Positions;
using SIRU.Core.Application.Services.Vacants;

namespace SIRU.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            MappingConfig.RegisterMappings();

            services.AddScoped<IVacantService, VacantService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<ICandidateService, CandidateService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ICriterionService, CriterionService>();
            services.AddScoped<IEvaluationService, EvaluationService>();
        }
    }
}
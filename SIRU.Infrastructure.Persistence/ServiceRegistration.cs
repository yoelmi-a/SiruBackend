using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Reports;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;
using SIRU.Infrastructure.Persistence.Repositories;

namespace SIRU.Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IGenericRepository<Vacant>, VacantRepository>();
        services.AddScoped<IGenericRepository<Department>, DepartmentRepository>();
        services.AddScoped<IGenericRepository<Position>, PositionRepository>();
        services.AddScoped<IGenericRepository<Candidate>, CandidateRepository>();
        services.AddScoped<IGenericRepository<Employee>, EmployeeRepository>();
        services.AddScoped<IGenericRepository<Criterion>, CriterionRepository>();
        services.AddScoped<IGenericRepository<Evaluation>, EvaluationRepository>();

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
    }
}
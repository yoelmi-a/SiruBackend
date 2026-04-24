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
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        services.AddScoped<IGenericRepository<Vacant>, VacantRepository>();
        services.AddScoped<IGenericRepository<Department>, DepartmentRepository>();
        services.AddScoped<IGenericRepository<Position>, PositionRepository>();
        services.AddScoped<IGenericRepository<Candidate>, CandidateRepository>();
        services.AddScoped<IGenericRepository<Employee>, EmployeeRepository>();
        services.AddScoped<IGenericRepository<Criterion>, CriterionRepository>();
        services.AddScoped<IGenericRepository<Evaluation>, EvaluationRepository>();

        services.AddScoped<IVacantRepository, VacantRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ICriterionRepository, CriterionRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IVacancyCandidateRepository, VacancyCandidateRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
    }
}
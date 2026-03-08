using Microsoft.EntityFrameworkCore;
using SIRU.Core.Domain.Entities;
using System.Reflection;

namespace SIRU.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Vacant> Vacants { get; set; }
        public DbSet<VacancyCandidate> VacancyCandidates { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<EmployeePosition> EmployeePositions { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Criterion> Criteria { get; set; }
        public DbSet<EvaluationCriterion> EvaluationCriteria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}

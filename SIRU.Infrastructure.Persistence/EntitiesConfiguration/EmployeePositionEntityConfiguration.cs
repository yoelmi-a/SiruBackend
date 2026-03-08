using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIRU.Core.Domain.Entities;

namespace SIRU.Infrastructure.Persistence.EntitiesConfiguration
{
    public class EmployeePositionEntityConfiguration : IEntityTypeConfiguration<EmployeePosition>
    {
        public void Configure(EntityTypeBuilder<EmployeePosition> builder)
        {
            builder.ToTable("EmployeePositions");
            builder.HasKey(ep => new { ep.PositionId, ep.EmployeeId, ep.StartDate });

            builder.HasOne(ep => ep.Employee)
                .WithMany(e => e.PositionsOccupied)
                .HasForeignKey(ep => ep.EmployeeId);

            builder.HasOne(ep => ep.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(ep => ep.PositionId);
        }
    }
}

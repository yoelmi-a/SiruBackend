using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIRU.Core.Domain.Entities;

namespace SIRU.Infrastructure.Persistence.EntitiesConfiguration
{
    public class EvaluationEntityConfiguration : IEntityTypeConfiguration<Evaluation>
    {
        public void Configure(EntityTypeBuilder<Evaluation> builder)
        {
            builder.ToTable("Evaluations");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Date).IsRequired();

            builder.HasOne(e => e.EmployeePosition)
                .WithMany(ep => ep.Evaluations)
                .HasForeignKey(e => e.EmployeePositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIRU.Core.Domain.Entities;

namespace SIRU.Infrastructure.Persistence.EntitiesConfiguration
{
    public class EvaluationCriterionEntityConfiguration : IEntityTypeConfiguration<EvaluationCriterion>
    {
        public void Configure(EntityTypeBuilder<EvaluationCriterion> builder)
        {
            builder.ToTable("EvaluationCriteria");
            builder.HasKey(ec => new { ec.EvaluationId, ec.CriteriaId });

            builder.HasOne(ec => ec.Evaluation)
                .WithMany(e => e.Criteria)
                .HasForeignKey(ec => ec.EvaluationId);

            builder.HasOne(ec => ec.Criterion)
                .WithMany(c => c.Evaluations)
                .HasForeignKey(ec => ec.CriteriaId);

            builder.Property(ec => ec.Score).IsRequired();
            builder.Property(ec => ec.Observation).HasMaxLength(500);
        }
    }
}

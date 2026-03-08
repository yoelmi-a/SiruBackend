using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIRU.Core.Domain.Entities;

namespace SIRU.Infrastructure.Persistence.EntitiesConfiguration
{
    public class VacancyCandidateEntityConfiguration : IEntityTypeConfiguration<VacancyCandidate>
    {
        public void Configure(EntityTypeBuilder<VacancyCandidate> builder)
        {
            builder.ToTable("VacancyCandidates");
            builder.HasKey(vc => new { vc.VacantId, vc.CandidateId });

            builder.HasOne(vc => vc.Vacant)
                .WithMany(v => v.Candidates)
                .HasForeignKey(vc => vc.VacantId);

            builder.HasOne(vc => vc.Candidate)
                .WithMany(c => c.VacanciesApplied)
                .HasForeignKey(vc => vc.CandidateId);

            builder.Property(vc => vc.Score).IsRequired();
            builder.Property(vc => vc.Status).IsRequired();
        }
    }
}

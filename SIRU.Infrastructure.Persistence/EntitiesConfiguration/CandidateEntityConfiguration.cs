using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIRU.Core.Domain.Entities;

namespace SIRU.Infrastructure.Persistence.EntitiesConfiguration
{
    public class CandidateEntityConfiguration : IEntityTypeConfiguration<Candidate>
    {
        public void Configure(EntityTypeBuilder<Candidate> builder)
        {
            builder.ToTable("Candidates");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Names).IsRequired().HasMaxLength(100);
            builder.Property(c => c.LastNames).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
            builder.Property(c => c.PhoneNumber).HasMaxLength(20);
            builder.Property(c => c.CvUrl).IsRequired();
        }
    }
}

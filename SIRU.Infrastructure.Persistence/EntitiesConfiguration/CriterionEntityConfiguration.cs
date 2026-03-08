using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIRU.Core.Domain.Entities;

namespace SIRU.Infrastructure.Persistence.EntitiesConfiguration
{
    public class CriterionEntityConfiguration : IEntityTypeConfiguration<Criterion>
    {
        public void Configure(EntityTypeBuilder<Criterion> builder)
        {
            builder.ToTable("Criteria");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        }
    }
}

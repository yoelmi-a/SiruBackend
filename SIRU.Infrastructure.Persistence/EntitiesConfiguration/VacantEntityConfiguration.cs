using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIRU.Core.Domain.Entities;

namespace SIRU.Infrastructure.Persistence.EntitiesConfiguration
{
    public class VacantEntityConfiguration : IEntityTypeConfiguration<Vacant>
    {
        public void Configure(EntityTypeBuilder<Vacant> builder)
        {
            builder.ToTable("Vacants");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.Title).IsRequired().HasMaxLength(150);
            builder.Property(v => v.Description).IsRequired();
            builder.Property(v => v.Profile).IsRequired();
            builder.Property(v => v.Status).IsRequired();
        }
    }
}

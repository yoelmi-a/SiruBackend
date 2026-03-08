using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIRU.Core.Domain.Entities;

namespace SIRU.Infrastructure.Persistence.EntitiesConfiguration
{
    public class EmployeeEntityConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Names).IsRequired().HasMaxLength(100);
            builder.Property(e => e.LastNames).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Email).IsRequired().HasMaxLength(150);
            builder.Property(e => e.IdCard).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Address).IsRequired().HasMaxLength(250);
            builder.Property(e => e.Birthdate).IsRequired();
            builder.Property(e => e.Status).IsRequired();
        }
    }
}

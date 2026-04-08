using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIRU.Infrastructure.Identity.Entities;

namespace SIRU.Infrastructure.Identity.Contexts;

public class AuthDbContext : IdentityDbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("Identity");
        builder.Entity<IdentityUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");

        builder.Entity<UserSession>().HasKey(x => x.Id);
        builder.Entity<UserSession>().Property(x => x.Id).HasMaxLength(36);
        builder.Entity<UserSession>().Property(x => x.AccountId).HasMaxLength(36);
        builder.Entity<UserSession>().Property(x => x.DeviceInfo).HasMaxLength(100);
        builder.Entity<UserSession>().Property(x => x.RefreshTokenHash).HasMaxLength(64);
    }
}
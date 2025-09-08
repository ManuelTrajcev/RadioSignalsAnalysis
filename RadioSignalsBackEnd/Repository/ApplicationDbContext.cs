using Domain.Domain_Models;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChannelFrequency> ChannelFrequencies { get; set; }
    public DbSet<ElectricFieldStrength> ElectricFieldStrengths { get; set; }
    public DbSet<GeoCoordinate> GeoCoordinates { get; set; }
    public DbSet<Measurement> Measurements { get; set; }
    public DbSet<Municipality> Municipalities { get; set; }
    public DbSet<Settlement> Settlements { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var roles = Enum.GetNames(typeof(Role)).Select(rName =>
        new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = rName,
            NormalizedName = rName.ToUpperInvariant()
        });

        builder.Entity<IdentityRole>().HasData(roles);

    }
}
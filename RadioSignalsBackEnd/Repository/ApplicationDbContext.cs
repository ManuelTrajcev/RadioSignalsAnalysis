using System.Threading.Channels;
using Domain.Domain_Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RadioSignalsWeb.Data;

public class ApplicationDbContext : IdentityDbContext
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
    public DbSet<User> User { get; set; }

}
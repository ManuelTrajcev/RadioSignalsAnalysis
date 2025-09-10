using Domain.Domain_Models;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Repository;

public class ApplicationDbContext : IdentityDbContext<User>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<ChannelFrequency> ChannelFrequencies { get; set; }
    public DbSet<ElectricFieldStrength> ElectricFieldStrengths { get; set; }
    public DbSet<GeoCoordinate> GeoCoordinates { get; set; }
    public DbSet<Measurement> Measurements { get; set; }
    public DbSet<Municipality> Municipalities { get; set; }
    public DbSet<Settlement> Settlements { get; set; }

    // NEW
    public DbSet<ReferenceThreshold> ReferenceThresholds { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        // Check constraint for ChannelFrequencies:
        // TV => ChannelNumber not null, FrequencyMHz null
        // FM => ChannelNumber null, FrequencyMHz not null
        builder.Entity<ChannelFrequency>()
            .ToTable(tb => tb.HasCheckConstraint(
                "CK_ChannelFrequencies_TV_or_FM",
                "(" +
                "(\"IsTvChannel\" = TRUE AND \"ChannelNumber\" IS NOT NULL AND \"FrequencyMHz\" IS NULL) " +
                "OR " +
                "(\"IsTvChannel\" = FALSE AND \"ChannelNumber\" IS NULL AND \"FrequencyMHz\" IS NOT NULL)" +
                ")"
            ));


        // Default FrequencyUnit to MHz (applies when not provided)
        builder.Entity<ChannelFrequency>()
            .Property(cf => cf.FrequencyUnit)
            .HasDefaultValue(FrequencyUnit.MHz);

        builder.Entity<GeoCoordinate>()
            .ToTable(tb => tb.HasCheckConstraint(
                "CK_GeoCoordinate_MinSec",
                "(\"LatitudeMinutes\" BETWEEN 0 AND 59) AND (\"LatitudeSeconds\" >= 0 AND \"LatitudeSeconds\" < 60) AND " +
                "(\"LongitudeMinutes\" BETWEEN 0 AND 59) AND (\"LongitudeSeconds\" >= 0 AND \"LongitudeSeconds\" < 60)"));

        builder.Entity<ElectricFieldStrength>()
            .ToTable(tb => tb.HasCheckConstraint(
                "CK_ElectricFieldStrength_Value_Positive",
                "\"Value\" >= 0"));

        builder.Entity<Measurement>()
            .HasOne(m => m.ElectricFieldStrength)
            .WithOne()
            .HasForeignKey<Measurement>(m => m.ElectricFieldStrengthId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure ALL enum properties (including nullable enums) are stored as strings
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var entity = builder.Entity(entityType.ClrType);
            foreach (var prop in entityType.ClrType.GetProperties())
            {
                var t = prop.PropertyType;
                var underlying = Nullable.GetUnderlyingType(t) ?? t;
                if (underlying.IsEnum)
                {
                    entity.Property(prop.Name).HasConversion<string>();
                }
            }
        }
    }

    // Global enum-to-string (non-nullable) convention as a safety net
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
        base.ConfigureConventions(configurationBuilder);
    }

    // Auto audit stamps
    public override int SaveChanges()
    {
        ApplyAuditStamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditStamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditStamps()
    {
        var now = DateTime.UtcNow;
        var user = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.UpdatedAtUtc = now;
                    entry.Entity.CreatedBy = user;
                    entry.Entity.UpdatedBy = user;
                    break;
                case EntityState.Modified:
                    // keep original CreatedAtUtc
                    entry.Property(e => e.CreatedAtUtc).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    entry.Entity.UpdatedAtUtc = now;
                    entry.Entity.UpdatedBy = user;
                    break;
            }
        }
    }
}

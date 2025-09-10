using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Domain_Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Repository.Seed;

public static class DbSeedExtensions
{
    // Seeds Municipalities & Settlements from a JSON file if none exist.
    // "SeedData/north_macedonia_municipalities_settlements_seed.json"
    public static async Task SeedMunicipalitiesAndSettlementsAsync(
        this IServiceProvider services,
        string relativeJsonPath,
        bool migrate = true)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("DbSeed");
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (migrate)
        {
            await db.Database.MigrateAsync();
        }

        // Short-circuit if already seeded
        if (await db.Municipalities.AsNoTracking().AnyAsync())
        {
            logger?.LogInformation("Municipalities already present. Skipping seed.");
            return;
        }

        var jsonPath = Path.IsPathRooted(relativeJsonPath)
            ? relativeJsonPath
            : Path.Combine(env.ContentRootPath, relativeJsonPath);

        if (!File.Exists(jsonPath))
        {
            logger?.LogError("Seed file not found at {Path}", jsonPath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonPath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var items = JsonSerializer.Deserialize<List<MunicipalitySeed>>(json, options) ?? new();
        if (items.Count == 0)
        {
            logger?.LogWarning("Seed file contained no items.");
            return;
        }

        // Build entities and insert
        foreach (var m in items)
        {
            if (string.IsNullOrWhiteSpace(m.Name)) continue;

            var municipality = new Municipality
            {
                Name = m.Name.Trim()
            };

            if (m.Settlements != null && m.Settlements.Count > 0)
            {
                foreach (var s in m.Settlements)
                {
                    if (string.IsNullOrWhiteSpace(s.Name)) continue;

                    municipality.Settlements.Add(new Settlement
                    {
                        Name = s.Name.Trim(),
                        RegistryNumber = string.IsNullOrWhiteSpace(s.RegistryNumber)
                            ? null
                            : s.RegistryNumber.Trim(),
                        Population = s.Population ?? 0,
                        Households = s.Households ?? 0
                    });
                }
            }

            db.Municipalities.Add(municipality);
        }

        await db.SaveChangesAsync();
        logger?.LogInformation("Seeded {Municipalities} municipalities with settlements.",
            await db.Municipalities.CountAsync());
    }
}

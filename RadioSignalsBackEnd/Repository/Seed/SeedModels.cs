using System.Collections.Generic;

namespace Repository.Seed;

public class MunicipalitySeed
{
    public string Name { get; set; } = default!;
    public List<SettlementSeed> Settlements { get; set; } = new();
}

public class SettlementSeed
{
    public string Name { get; set; } = default!;
    public string? RegistryNumber { get; set; }
    public int? Population { get; set; }
    public int? Households { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Trim.Models;

public class Vehicle
{
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; }

    [Required, MaxLength(50)]
    public string Vin { get; set; } = default!;

    public VehicleStatusEnum Status { get; set; } = VehicleStatusEnum.AVAILABLE;

    public VehicleConfiguration? Configuration { get; set; }

    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public TransportCompany? TransportCompany { get; set; }
}

public class VehicleConfiguration
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string BaseModel { get; set; } = default!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    // M:N via join entity
    public ICollection<VehicleConfigurationOption> Options { get; set; } = new List<VehicleConfigurationOption>();
}

public class Option
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string OptionCode { get; set; } = default!;

    [MaxLength(400)]
    public string? Description { get; set; }

    [Precision(18, 2)]
    public decimal Price { get; set; }

    public ICollection<VehicleConfigurationOption> Configurations { get; set; } = new List<VehicleConfigurationOption>();

    // Relationships to rules (option1/option2)
    public ICollection<CompatibilityRule> RulesAsOption1 { get; set; } = new List<CompatibilityRule>();
    public ICollection<CompatibilityRule> RulesAsOption2 { get; set; } = new List<CompatibilityRule>();
}

public class VehicleConfigurationOption
{
    public int VehicleConfigurationId { get; set; }
    public VehicleConfiguration VehicleConfiguration { get; set; } = default!;

    public int OptionId { get; set; }
    public Option Option { get; set; } = default!;
}

public class CompatibilityRule
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string Type { get; set; } = default!; // DISALLOWED / REQUIRED / CONDITIONAL

    public int Option1Id { get; set; }
    public Option Option1 { get; set; } = default!;

    public int Option2Id { get; set; }
    public Option Option2 { get; set; } = default!;
}

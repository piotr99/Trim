using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Trim.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = default!;

    [Required, MaxLength(50)]
    public string Vin { get; set; } = default!;

    public VehicleStatusEnum Status { get; set; } = VehicleStatusEnum.AVAILABLE;

    public int? ConfigurationId { get; set; }
    public VehicleConfiguration? Configuration { get; set; }

    // Klient
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Pojazd należy do JEDNEJ oferty (NIE kolekcji)
    public int? OfferId { get; set; }
    public Offer? Offer { get; set; }

    // Pojazd należy do JEDNEGO zamówienia (NIE kolekcji)
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
}

public class VehicleConfiguration
{
    public int Id { get; set; }

    public int SizeId { get; set; }
    [ValidateNever, BindNever] public VehicleCabSize Size { get; set; } = default!;

    public int EngineId { get; set; }
    [ValidateNever, BindNever] public VehicleEngine Engine { get; set; } = default!;
    public int GearboxId { get; set; }
    [ValidateNever, BindNever] public VehicleGearbox Gearbox { get; set; } = default!;
    public int InteriorId { get; set; }
    [ValidateNever, BindNever] public VehicleInterior Interior { get; set; } = default!;
    public int DrivetrainId { get; set; }
    [ValidateNever, BindNever] public VehicleDrivetrain Drivetrain { get; set; } = default!;
    [BindNever] public decimal Price { get; set; }
    [BindNever] public decimal Bonus { get; set; }
    [BindNever] public decimal BonusMultiplier { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public string? AdditionalEquipment { get; set; }
}
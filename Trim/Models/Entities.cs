using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

    public ICollection<OfferVehicle> OfferVehicles { get; set; } = new List<OfferVehicle>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public Customer? Customer { get; set; }
    public int? CustomerId { get; set; }
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

public class OfferVehicleConfiguration : VehicleConfiguration
{
    public int OfferId { get; set; }
    [ValidateNever, BindNever] public Offer Offer { get; set; }
}

public class OrderVehicleConfiguration : VehicleConfiguration
{
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
}
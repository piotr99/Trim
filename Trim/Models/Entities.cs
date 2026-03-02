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

    public ICollection<OfferVehicle> OfferVehicles { get; set; } = new List<OfferVehicle>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public Customer? Customer { get; set; }
    public int? CustomerId { get; set; }
}

public class VehicleConfiguration
{
    public int Id { get; set; }

    public VehicleCabSize Size { get; set; }
    public VehicleEngine Engine { get; set; }
    public VehicleGearbox Gearbox { get; set; }
    public VehicleInterior Interior { get; set; }
    public VehicleDrivetrain Drivetrain { get; set; }
    
    public string? AdditonalEquipment { get; set; }
    public int? AdditionalPrice { get; set; }

}

public class OfferVehicleConfiguration : VehicleConfiguration
{
    public int OfferId { get; set; }
    public Offer Offer { get; set; }
}

public class OrderVehicleConfiguration : VehicleConfiguration
{
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
}
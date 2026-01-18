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
    public TransportCompany? TransportCompany { get; set; }
    public Customer? Customer { get; set; }
    public int? CustomerId { get; set; }
}

public class VehicleConfiguration
{
    public int Id { get; set; }

    public VehicleCabSizeEnum Size { get; set; }
    public VehicleEngineEnum Engine { get; set; }
    public VehicleGeraboxEnum Gerabox { get; set; }
    public VehicleInteriorEnum Interior { get; set; }
    public VehicleDrivetrainEnum Drivetrain { get; set; }
    
    public int OptionId { get; set; }
    public Option? Option { get; set; }

}

public class Option
{
    public int Id { get; set; }

    [MaxLength(400)]
    public string? Description { get; set; }

    [Precision(18, 2)]
    public decimal Price { get; set; }
}

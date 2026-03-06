using Trim.Models;

namespace Trim.Models.ViewModels;

public class EditOfferVm
{
    public int OfferId { get; set; }
    public int CustomerId { get; set; }
    public string OfferFriendlyName { get; set; } = default!;
    public OfferStatusEnum Status { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal Bonus { get; set; }

    public List<OfferVehicleConfiguration> Vehicles { get; set; } = new();
    public List<VehicleCabSize> VehicleCabSizes { get; set; } = new();
    public List<VehicleDrivetrain> VehicleDrivetrains { get; set; } = new();
    public List<VehicleEngine> VehicleEngines { get; set; } = new();
    public List<VehicleGearbox> VehicleGearboxes { get; set; } = new();
    public List<VehicleInterior> VehicleInteriors { get; set; } = new();
}
public class OfferVehicleConfigInput
{
    public int Id { get; set; }
    public int SizeId { get; set; }
    public int EngineId { get; set; }
    public int GearboxId { get; set; }
    public int InteriorId { get; set; }
    public int DrivetrainId { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public string? AdditionalEquipment { get; set; }
}
namespace Trim.Models;

public class OfferVehicle
{
    public int OfferId { get; set; }
    public Offer Offer { get; set; } = default!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;
}

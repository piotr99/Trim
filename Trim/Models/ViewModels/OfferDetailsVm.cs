namespace Trim.Models.ViewModels;

public class OfferDetailsVm
{
    public Offer Offer { get; set; } = default!;
    public List<OfferVehicleConfiguration> OfferVehicleConfigurations { get; set; } = new();
}

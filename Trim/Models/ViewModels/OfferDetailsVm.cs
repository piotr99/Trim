namespace Trim.Models.ViewModels;

public class OfferDetailsVm
{
    public Offer Offer { get; set; } = default!;
    public List<OfferVersion> OfferVersions { get; set; } = new();
    public List<OfferVehicleConfiguration> OfferVehicleConfigurations { get; set; } = new();
}

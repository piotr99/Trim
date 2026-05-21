using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trim.DbContext;
using Trim.Models;
using Trim.Models.BussinesCalculactions;

namespace Trim.Helpers;

public interface IOfferCalculator
{
    Task<Calculations> GetCalculationsAsync(List<VehicleConfigDto> configs, decimal discount);
}

public class OfferCalculator : IOfferCalculator
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OfferCalculator> _logger;

    public OfferCalculator(ApplicationDbContext db, ILogger<OfferCalculator> logger)
    {
        _db = db;
        _logger = logger;
    }

    private async Task LoadVehicleComponentsAsync()
    {
        VehicleCabSizes = await _db.VehicleCabSizes.AsNoTracking().ToListAsync();
        VehicleEngines = await _db.VehicleEngines.AsNoTracking().ToListAsync();
        VehicleGearboxes = await _db.VehicleGearboxes.AsNoTracking().ToListAsync();
        VehicleInteriors = await _db.VehicleInteriors.AsNoTracking().ToListAsync();
        VehicleDrivetrains = await _db.VehicleDrivetrains.AsNoTracking().ToListAsync();
    }

    public List<VehicleCabSize> VehicleCabSizes { get; private set; } = new();
    public List<VehicleEngine> VehicleEngines { get; private set; } = new();
    public List<VehicleGearbox> VehicleGearboxes { get; private set; } = new();
    public List<VehicleInterior> VehicleInteriors { get; private set; } = new();
    public List<VehicleDrivetrain> VehicleDrivetrains { get; private set; } = new();

    public async Task<Calculations> GetCalculationsAsync(List<VehicleConfigDto> configs, decimal discount)
    {
        if (configs == null || !configs.Any())
        {
            _logger.LogWarning("Próba kalkulacji oferty dla pustej listy pojazdów. Zastosowany rabat: {Discount}", discount);
            return new Calculations(new List<VehiclesTemp>(), 0, 0, -discount);
        }

        _logger.LogInformation("Rozpoczęto kalkulację cen dla {VehicleCount} pojazdów z rabatem {Discount} PLN.", configs.Count, discount);

        try
        {
            await LoadVehicleComponentsAsync();

            var vehicles = new List<VehiclesTemp>();
            decimal totalPrice = 0m;
            decimal totalBonus = 0m;

            foreach (var c in configs)
            {
                decimal price = 0m;
                decimal bonusMultiplier = 1m;

                var size = VehicleCabSizes.FirstOrDefault(x => x.Id == c.size);
                var eng = VehicleEngines.FirstOrDefault(x => x.Id == c.engine);
                var gbx = VehicleGearboxes.FirstOrDefault(x => x.Id == c.gearbox);
                var intr = VehicleInteriors.FirstOrDefault(x => x.Id == c.interior);
                var drv = VehicleDrivetrains.FirstOrDefault(x => x.Id == c.drivetrain);


                if (c.size > 0 && size == null) _logger.LogWarning("Brak kabiny o ID {CabSizeId} w bazie słownikowej podczas wyliczeń.", c.size);
                if (c.engine > 0 && eng == null) _logger.LogWarning("Brak silnika o ID {EngineId} w bazie słownikowej podczas wyliczeń.", c.engine);
                if (c.gearbox > 0 && gbx == null) _logger.LogWarning("Brak skrzyni biegów o ID {GearboxId} w bazie słownikowej podczas wyliczeń.", c.gearbox);

                price += (size?.Price ?? 0)
                      + (eng?.Price ?? 0)
                      + (gbx?.Price ?? 0)
                      + (intr?.Price ?? 0)
                      + (drv?.Price ?? 0);

                price += (c.additionalPrice ?? 0m);

                bonusMultiplier += (size?.BonusMultiplier ?? 0)
                                + (eng?.BonusMultiplier ?? 0)
                                + (gbx?.BonusMultiplier ?? 0)
                                + (intr?.BonusMultiplier ?? 0)
                                + (drv?.BonusMultiplier ?? 0);

                var vehicleBonus = price * 0.02m * bonusMultiplier;

                totalPrice += price;
                totalBonus += vehicleBonus;

                vehicles.Add(new VehiclesTemp
                {
                    Price = price,
                    Bonus = vehicleBonus,
                    BonusMultiplier = bonusMultiplier
                });
            }

            var finalPrice = Math.Max(0, totalPrice - discount);
            totalBonus -= discount;

            _logger.LogInformation("Zakończono kalkulację. Cena bazowa: {TotalPrice}, Po rabacie: {FinalPrice}, Łączny bonus: {TotalBonus}",
                totalPrice, finalPrice, totalBonus);

            return new Calculations(vehicles, totalPrice, finalPrice, totalBonus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wystąpił krytyczny błąd podczas kalkulacji oferty dla {VehicleCount} pojazdów (Rabat: {Discount}).", configs.Count, discount);

            throw;
        }
    }
}
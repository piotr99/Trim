using System.Text.Json;
using Trim.DbContext;
using Trim.Models;
using Microsoft.EntityFrameworkCore;
namespace Trim.Helpers;



public interface IOfferCalculator
{
    Task<(decimal Price, decimal FinalPrice, decimal Margin)> GetCalculationsAsync(string payload, int discount);
}

public class OfferCalculator : IOfferCalculator
{
    private readonly ApplicationDbContext _db;

    public OfferCalculator(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(decimal Price, decimal FinalPrice, decimal Margin)> GetCalculationsAsync(string payload, int discount)
    {
        
        
        var configs = JsonSerializer.Deserialize<List<VehicleConfigDto>>(payload)
                      ?? new List<VehicleConfigDto>();

        // Bezpieczne parsowanie string -> int
        static int ToInt(string? s, int def = 0) => int.TryParse(s, out var v) ? v : def;

        var keys = new HashSet<(string Group, int EnumValue)>();
        foreach (var c in configs)
        {
            int sizeInt = ToInt(c.size);
            var cabSize = await _db.VehicleCabSizes.FirstAsync(x => x.Id == sizeInt);
            int engineInt = ToInt(c.engine);
            var engineSize = await _db.VehicleEngines.FirstAsync(x => x.Id == engineInt);
            int drivetrainInt = ToInt(c.drivetrain);
            var drivetrain = await _db.VehicleDrivetrains.FirstAsync(x => x.Id == drivetrainInt);
            int gearboxInt = ToInt(c.gearbox);
            var gearbox  = await _db.VehicleGearboxes.FirstAsync(x => x.Id == gearboxInt);
            int interiorInt = ToInt(c.interior);
            var interior = await _db.VehicleInteriors.FirstAsync(x => x.Id == interiorInt);
        }

        
        foreach (var c in configs)
        {
            keys.Add(("CabSize", ToInt(c.size)));
            keys.Add(("Engine", ToInt(c.engine)));
            keys.Add(("Gearbox", ToInt(c.gearbox)));
            keys.Add(("Interior", ToInt(c.interior)));
            keys.Add(("Drivetrain", ToInt(c.drivetrain)));
        }
        
        var groups = keys.Select(k => k.Group).Distinct().ToList();

        var prices = await _db.OptionPrices.AsNoTracking()
            .Where(p => groups.Contains(p.Group))
            .Select(p => new { p.Group, p.EnumValue, p.Price })
            .ToListAsync();

        var dict = prices.ToDictionary(x => (x.Group, x.EnumValue), x => x.Price);

        decimal price = 0m;
        foreach (var key in keys)
        {
            foreach (var item in dict)
            {
                if (key.Group == item.Key.Group && key.EnumValue == item.Key.EnumValue)
                {
                    price += item.Value;
                }
            }
        }
        decimal finalPrice = price - discount;
        decimal margin = price * 0.01m;

        return (price, finalPrice, margin);
    }
}



    public class VehicleConfigDto
    {
        public string? size { get; set; }
        public string? engine { get; set; }
        public string? gearbox { get; set; }
        public string? interior { get; set; }
        public string? drivetrain { get; set; }
    }

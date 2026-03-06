namespace Trim.Models.BussinesCalculactions;

public class CalculationRequest 
{
    public List<VehicleConfigDto> Configs { get; set; } = new();
    public decimal Discount { get; set; }
}

public class VehicleConfigDto
{ public int? size { get; set; }
    public int? engine { get; set; }
    public int? gearbox { get; set; }
    public int? interior { get; set; }
    public int? drivetrain { get; set; }
    public decimal? additionalPrice { get; set; }
}

public class VehiclesTemp
{
    public decimal Price { get; set; }
    public decimal Bonus { get; set; }
    public decimal BonusMultiplier { get; set; }
}

public class Calculations
{
    public List<VehiclesTemp> Vehicles { get; set; }
    public decimal Price { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal Bonus { get; set; }

    public Calculations(List<VehiclesTemp> vehicles, decimal price, decimal finalPrice, decimal bonus)
    {
        Vehicles = vehicles;
        Price = price;
        FinalPrice = finalPrice;
        Bonus = bonus;
    }
}

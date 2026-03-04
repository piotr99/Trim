namespace Trim.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

public class VehicleCabSize
{
    public int Id { get; set;}
    public string Name { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal Price { get; set; }
    [Precision(18, 2)]
    public decimal BonusMultiplier { get; set; } = 0;
}

public class VehicleEngine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal Price { get; set; }
    [Precision(18, 2)]
    public decimal BonusMultiplier { get; set; } = 0;
}

public class VehicleGearbox
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal Price { get; set; }
    [Precision(18, 2)]
    public decimal BonusMultiplier { get; set; } = 0;
}

public class VehicleInterior
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal Price { get; set; }

    [Precision(18, 2)] 
    public decimal BonusMultiplier { get; set; } = 0;
}

public class VehicleDrivetrain
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal Price { get; set; }
    [Precision(18, 2)]
    public decimal BonusMultiplier { get; set; } = 0;
}
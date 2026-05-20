namespace Trim.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

public class VehicleConfiguration
{
    public int Id { get; set; }

    public int SizeId { get; set; }
    [ValidateNever, BindNever] public VehicleCabSize Size { get; set; } = default!;

    public int EngineId { get; set; }
    [ValidateNever, BindNever] public VehicleEngine Engine { get; set; } = default!;
    public int GearboxId { get; set; }
    [ValidateNever, BindNever] public VehicleGearbox Gearbox { get; set; } = default!;
    public int InteriorId { get; set; }
    [ValidateNever, BindNever] public VehicleInterior Interior { get; set; } = default!;
    public int DrivetrainId { get; set; }
    [ValidateNever, BindNever] public VehicleDrivetrain Drivetrain { get; set; } = default!;
    [BindNever] public decimal Price { get; set; }
    [BindNever] public decimal Bonus { get; set; }
    [BindNever] public decimal BonusMultiplier { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public string? AdditionalEquipment { get; set; }
}
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
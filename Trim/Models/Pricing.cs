using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Trim.Models;

public class PriceList
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;

    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }

    public ICollection<PriceListItem> Items { get; set; } = new List<PriceListItem>();
}

public class PriceListItem
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string ProductCode { get; set; } = default!;

    [Precision(18, 2)]
    public decimal BasePrice { get; set; }

    public int PriceListId { get; set; }
    public PriceList PriceList { get; set; } = default!;
}

public class FleetDiscount
{
    public int Id { get; set; }
    public int QuantityThreshold { get; set; }

    [Precision(5, 2)]
    public decimal DiscountPercent { get; set; }
}

public class SalesBonus
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;

    [Precision(5, 2)]
    public decimal Percent { get; set; }

    [MaxLength(1000)]
    public string? Conditions { get; set; }
}
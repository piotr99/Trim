using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Trim.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = default!;

    [Required, MaxLength(50)]
    public string Vin { get; set; } = default!;
    public VehicleStatusEnum Status { get; set; } = VehicleStatusEnum.AVAILABLE;

    public int? ConfigurationId { get; set; }
    public VehicleConfiguration? Configuration { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? OfferId { get; set; }
    public Offer? Offer { get; set; }
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
}


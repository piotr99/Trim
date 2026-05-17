using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Http.HttpResults;


namespace Trim.Models;

public class Offer
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string OfferFriendlyName { get; set; } = default!;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public OfferStatusEnum Status { get; set; } = OfferStatusEnum.DRAFT;

    // OFERTA ma WIELE pojazdów
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public decimal FinalPrice { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal Bonus { get; set; }

    public int SalesCaseId { get; set; }
    public SalesCase SalesCase { get; set; } = default!;
}
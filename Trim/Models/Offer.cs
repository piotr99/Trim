using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace Trim.Models;

public class Offer
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string OfferFriendlyName { get; set; } = default!;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public OfferStatusEnum Status { get; set; } = OfferStatusEnum.DRAFT;

    public int CustomerId { get; set; }
    [ValidateNever, BindNever]
    public Customer Customer { get; set; } = default!;
    [ValidateNever, BindNever]
    public ICollection<OfferVehicleConfiguration> OfferVehicleConfigurations { get; set; } = new List<OfferVehicleConfiguration>();
    public int SalespersonId { get; set; }
    [ValidateNever, BindNever]
    public Salesperson Salesperson { get; set; } = default!;
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
    public PdfDocument? PdfDocument { get; set; }
    public ICollection<CustomerCommunication> Communications { get; set; } = new List<CustomerCommunication>();
    //kasa
    [Precision(18, 2)]
    public decimal FinalPrice { get; set; }
    [Precision(18, 2)]
    public decimal Price { get; set; }
    [Precision(18, 2)]
    public decimal Discount { get; set; }
    [Precision(18, 2)]
    public decimal Bonus { get; set; }
}


public class Order
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = default!;

    public OrderStatusEnum Status { get; set; } = OrderStatusEnum.NEW;

    [Precision(18, 2)]
    public decimal FinalPrice { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    public int SalespersonId { get; set; }
    public Salesperson Salesperson { get; set; } = default!;

    public Invoice? Invoice { get; set; }
}

public class Invoice
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = default!;

    public DateTime SaleDate { get; set; }
    public DateTime DueDate { get; set; }

    [Precision(18, 2)]
    public decimal GrossAmount { get; set; }

    public InvoiceStatusEnum Status { get; set; } = InvoiceStatusEnum.UNPAID;

    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public ICollection<CustomerCommunication> Communications { get; set; } = new List<CustomerCommunication>();
}

public class PdfDocument
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OfferId { get; set; }
    public Offer Offer { get; set; } = default!;
}

public class CustomerCommunication
{
    public int Id { get; set; }

    public MessageTypeEnum Type { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? DeliveryStatus { get; set; }

    // UML: Offer -> 0..*, Invoice -> 0..*, Customer -> 0..*
    public int? OfferId { get; set; }
    public Offer? Offer { get; set; }

    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}

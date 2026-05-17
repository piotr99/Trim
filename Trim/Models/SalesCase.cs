using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Trim.Models;

public class SalesCase
{
    public int Id { get; set; }
    public string CaseNumber { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;

    public SalesCaseStatusEnum Status { get; set; } = SalesCaseStatusEnum.NEW;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public int AssignedSalespersonId { get; set; }
    public Salesperson AssignedSalesperson { get; set; } = default!;

    // ZMIANA: Zamiast ICollection, mamy pojedyncze obiekty
    public Offer? Offer { get; set; }
    public Order? Order { get; set; }
    public ICollection<CustomerCommunication> ActivityLogs { get; set; } = new List<CustomerCommunication>();
}

public enum SalesCaseStatusEnum
{
    NEW,
    QUALIFICATION,
    NEGOTIATION,
    AWAITING_VEHICLE,
    CLOSED_WON,
    CLOSED_LOST
}
using System.ComponentModel.DataAnnotations;
namespace Trim.Models;

public enum LeadStatusEnum
{
    NEW = 0,
    IN_PROGRESS = 1,
    CLOSED = 2
}

public enum VehicleStatusEnum
{
    AVAILABLE = 0,
    RESERVED = 1,
    ORDERED = 2,
    DELIVERED = 3,
    OFFERED = 4,
    DRAFT = 5,
}

public enum OfferStatusEnum
{
    DRAFT = 0,
    SENT = 1,
    IN_NEGOTIATION = 2,
    ACCEPTED = 3,
    CANCELLED = 4
}

public enum OrderStatusEnum
{
    NEW = 0,
    IN_PROGRESS = 1,
    ORDERED = 2,
    COMPLETED = 3,
    CANCELLED = 4
}

public enum InvoiceStatusEnum
{
    UNPAID = 0,
    PARTIALLY_PAID = 1,
    PAID = 2,
    OVERDUE = 3
}

public enum MessageTypeEnum
{
    OFFER_SENT = 0,
    PAYMENT_REMINDER = 1,
    ORDER_STATUS_CHANGED = 2
}
public enum VehicleCabSizeEnum
{
    [Display(Name = "P - niska (P-cab)")]
    PCab = 0,

    [Display(Name = "G - normalna (G-cab)")]
    GCab = 1,

    [Display(Name = "R - wysoka (R-cab)")]
    RCab = 2,

    [Display(Name = "S - najwyższa (S-cab)")]
    SCab = 3,

    [Display(Name = "L - długa (L-cab)")]
    LCab = 4
}

public enum VehicleEngineEnum
{
    [Display(Name = "9L 360 KM")]
    L09 = 0,

    [Display(Name = "13L 560 KM")]
    L13 = 1,

    [Display(Name = "16L V8 770 KM")]
    V8_16 = 2
}

public enum VehicleGearboxEnum
{
    [Display(Name = "Manual (G-series)")]
    Manual = 0,

    [Display(Name = "Opticruise / Automated (AMT)")]
    Opticruise = 1,

    [Display(Name = "Allison (automat - zastosowania specjalne)")]
    AllisonAutomatic = 2
}

public enum VehicleInteriorEnum
{
    [Display(Name = "Standard")]
    Standard = 0,

    [Display(Name = "Comfort")]
    Comfort = 1,

    [Display(Name = "Premium / Highline")]
    Premium = 2,

    [Display(Name = "Luxury / Topline")]
    Luxury = 3
}

public enum VehicleDrivetrainEnum
{
    [Display(Name = "4x2")]
    _4x2 = 0,

    [Display(Name = "6x2")]
    _6x2 = 1,

    [Display(Name = "6x4")]
    _6x4 = 2,

    [Display(Name = "8x2")]
    _8x2 = 3,

    [Display(Name = "8x4")]
    _8x4 = 4
}
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
    DELIVERED = 3
}

public enum OfferStatusEnum
{
    DRAFT = 0,
    IN_NEGOTIATION = 1,
    ACCEPTED = 2,
    CANCELLED = 3
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
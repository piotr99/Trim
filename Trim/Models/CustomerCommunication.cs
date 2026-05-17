using System.ComponentModel.DataAnnotations;

namespace Trim.Models
{
    public class CustomerCommunication
    {
        public int Id { get; set; }
        [Required]
        public string MessageContent { get; set; } = default!;
        public bool IsPrivateMessage { get; set; }
        public MessageDirectionEnum Direction { get; set; }
        public int? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string? DeliveryStatus { get; set; }
        public int SalesCaseId { get; set; }
        public SalesCase SalesCase { get; set; } = default!;
    }

    public enum MessageDirectionEnum
    {
        OUTBOUND,  // Od nas do klienta
        INBOUND,   // Od klienta do nas
        INTERNAL,  // Wewnętrzna notatka (klient tego nie widzi)
        SYSTEM     // Powiadomienie automatyczne
    }

}

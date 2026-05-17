using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Trim.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(50)]
        public string OrderNumber { get; set; } = default!;

        public OrderStatusEnum Status { get; set; } = OrderStatusEnum.NEW;

        public decimal FinalPrice { get; set; }

        // ZAMÓWIENIE ma WIELE pojazdów (Usunięto VehicleId, bo to tabela Vehicle trzyma klucz)
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        public int SalesCaseId { get; set; }
        public SalesCase SalesCase { get; set; } = default!;
    }
}

using System.ComponentModel.DataAnnotations;

namespace Trim.Models;

public class Customer : ApplicationUser
    {

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(400)]
        public string? Address { get; set; }
        public string TaxId { get; set; }
        
        public int? SalespersonId { get; set; }
        public Salesperson? Salesperson { get; set; }

        public ICollection<SalesCase> SalesCases { get; set; } = new List<SalesCase>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
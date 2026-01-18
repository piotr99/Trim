using System.ComponentModel.DataAnnotations;

namespace Trim.Models;

public class TransportCompany
{
    public int Id { get; set; }

    [Required, MaxLength(200)] public string Name { get; set; } = default!;

    [Required, MaxLength(20)] public string TaxId { get; set; } = default!; // NIP

    [MaxLength(400)] public string? Address { get; set; }

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}

public class Customer
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = default!;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = default!;

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(400)]
        public string? Address { get; set; }

        public int? TransportCompanyId { get; set; }
        public TransportCompany? TransportCompany { get; set; }
        
        public int? SalespersonId { get; set; }
        public Salesperson? Salesperson { get; set; }

        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CustomerCommunication> Communications { get; set; } = new List<CustomerCommunication>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }

public class Lead
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = default!;

    [MaxLength(20)]
    public string? TaxId { get; set; } // NIP

    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    public LeadStatusEnum Status { get; set; } = LeadStatusEnum.NEW;

    public int? TransportCompanyId { get; set; }
    public TransportCompany? TransportCompany { get; set; }
}
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Trim.Models;

public class ApplicationUser : IdentityUser<int>
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    // Identity already has: Email, PhoneNumber, UserName (login)
    // Password is handled by Identity (hash) – NEVER store "Password" yourself.
}

public class Salesperson : ApplicationUser
{
    public ICollection<Offer> CreatedOffers { get; set; } = new List<Offer>();
    public ICollection<Order> ManagedOrders { get; set; } = new List<Order>();

    public ICollection<SalespersonSales> SoldVehicles { get; set; } = new List<SalespersonSales>();
}

public class SalesAdministrator : ApplicationUser
{
}

public class Administrator : ApplicationUser
{
}

//Do liczenia bonusu
public class SalespersonSales
{
    public int SalespersonId { get; set; }
    public Salesperson Salesperson { get; set; } = default!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    public int Quantity { get; set; }
}
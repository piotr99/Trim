using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Trim.Models;

namespace Trim.DbContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSety (EN)
    public DbSet<TransportCompany> TransportCompanies => Set<TransportCompany>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Lead> Leads => Set<Lead>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleConfiguration> VehicleConfigurations => Set<VehicleConfiguration>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<VehicleConfigurationOption> VehicleConfigurationOptions => Set<VehicleConfigurationOption>();
    public DbSet<CompatibilityRule> CompatibilityRules => Set<CompatibilityRule>();

    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OfferVersion> OfferVersions => Set<OfferVersion>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PdfDocument> PdfDocuments => Set<PdfDocument>();
    public DbSet<CustomerCommunication> CustomerCommunications => Set<CustomerCommunication>();

    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<FleetDiscount> FleetDiscounts => Set<FleetDiscount>();
    public DbSet<SalesBonus> SalesBonuses => Set<SalesBonus>();

    public DbSet<SalespersonSales> SalespersonSales => Set<SalespersonSales>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ===== Identity inheritance (TPH) =====
        // If ApplicationUser is abstract -> remove HasValue<ApplicationUser>("User")
        b.Entity<ApplicationUser>()
            .HasDiscriminator<string>("UserType")
            .HasValue<ApplicationUser>("User")
            .HasValue<Salesperson>("Salesperson")
            .HasValue<SalesAdministrator>("SalesAdministrator")
            .HasValue<SalesAdministrator>("Administrator");
        
        // ===== Customers -> Salesperson =====
        b.Entity<Customer>()
            .HasOne(c => c.Salesperson)
            .WithMany() // jeśli nie masz kolekcji po stronie Salesperson
            .HasForeignKey(c => c.SalespersonId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== TransportCompany -> Customers/Leads =====
        b.Entity<TransportCompany>()
            .HasIndex(x => x.TaxId)
            .IsUnique();

        b.Entity<TransportCompany>()
            .HasMany(x => x.Customers)
            .WithOne(x => x.TransportCompany)
            .HasForeignKey(x => x.TransportCompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<TransportCompany>()
            .HasMany(x => x.Leads)
            .WithOne(x => x.TransportCompany)
            .HasForeignKey(x => x.TransportCompanyId)
            .OnDelete(DeleteBehavior.SetNull);
        


        // ===== Vehicle <-> VehicleConfiguration (1:1) =====
        b.Entity<Vehicle>()
            .HasIndex(x => x.Vin)
            .IsUnique();

        b.Entity<VehicleConfiguration>()
            .HasOne(x => x.Vehicle)
            .WithOne(x => x.Configuration)
            .HasForeignKey<VehicleConfiguration>(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== VehicleConfiguration <-> Options (M:N via join) =====
        b.Entity<VehicleConfigurationOption>()
            .HasKey(x => new { x.VehicleConfigurationId, x.OptionId });

        b.Entity<VehicleConfigurationOption>()
            .HasOne(x => x.VehicleConfiguration)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.VehicleConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<VehicleConfigurationOption>()
            .HasOne(x => x.Option)
            .WithMany(x => x.Configurations)
            .HasForeignKey(x => x.OptionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Option>()
            .HasIndex(x => x.OptionCode)
            .IsUnique();

        // ===== CompatibilityRule: two FKs to Option =====
        b.Entity<CompatibilityRule>()
            .HasOne(r => r.Option1)
            .WithMany(o => o.RulesAsOption1)
            .HasForeignKey(r => r.Option1Id)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<CompatibilityRule>()
            .HasOne(r => r.Option2)
            .WithMany(o => o.RulesAsOption2)
            .HasForeignKey(r => r.Option2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Offer -> OfferVersions (1..*) =====
        b.Entity<Offer>()
            .HasIndex(o => o.OfferNumber)
            .IsUnique();

        b.Entity<Offer>()
            .HasMany(o => o.Versions)
            .WithOne(v => v.Offer)
            .HasForeignKey(v => v.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Offer -> Order (0..1)
        b.Entity<Offer>()
            .HasOne(o => o.Order)
            .WithOne()
            .HasForeignKey<Offer>(o => o.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // Offer -> PdfDocument (0..1) (1:1)
        b.Entity<PdfDocument>()
            .HasOne(d => d.Offer)
            .WithOne(o => o.PdfDocument)
            .HasForeignKey<PdfDocument>(d => d.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Offer -> Customer / Vehicle / Salesperson
        b.Entity<Offer>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Offers)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        

        b.Entity<Offer>()
            .HasOne(o => o.Vehicle)
            .WithMany(v => v.Offers)
            .HasForeignKey(o => o.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Offer>()
            .HasOne(o => o.Salesperson)
            .WithMany(s => s.CreatedOffers)
            .HasForeignKey(o => o.SalespersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Order -> Invoice (0..1) (1:1) =====
        b.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        b.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        b.Entity<Invoice>()
            .HasOne(i => i.Order)
            .WithOne(o => o.Invoice)
            .HasForeignKey<Invoice>(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order -> Customer / Vehicle / Salesperson
        b.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Order>()
            .HasOne(o => o.Vehicle)
            .WithMany(v => v.Orders)
            .HasForeignKey(o => o.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Order>()
            .HasOne(o => o.Salesperson)
            .WithMany(s => s.ManagedOrders)
            .HasForeignKey(o => o.SalespersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== CustomerCommunication (optional relationships) =====
        // nullable FKs -> SetNull avoids multiple cascade paths
        b.Entity<CustomerCommunication>()
            .HasOne(c => c.Offer)
            .WithMany(o => o.Communications)
            .HasForeignKey(c => c.OfferId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<CustomerCommunication>()
            .HasOne(c => c.Invoice)
            .WithMany(i => i.Communications)
            .HasForeignKey(c => c.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<CustomerCommunication>()
            .HasOne(c => c.Customer)
            .WithMany(cu => cu.Communications)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== PriceList -> PriceListItems (1..*) =====
        b.Entity<PriceList>()
            .HasMany(p => p.Items)
            .WithOne(i => i.PriceList)
            .HasForeignKey(i => i.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique product code inside a single price list
        b.Entity<PriceListItem>()
            .HasIndex(x => new { x.PriceListId, x.ProductCode })
            .IsUnique();

        // ===== SalespersonSales (map) =====
        b.Entity<SalespersonSales>()
            .HasKey(x => new { x.SalespersonId, x.VehicleId });

        b.Entity<SalespersonSales>()
            .HasOne(x => x.Salesperson)
            .WithMany(s => s.SoldVehicles)
            .HasForeignKey(x => x.SalespersonId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<SalespersonSales>()
            .HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

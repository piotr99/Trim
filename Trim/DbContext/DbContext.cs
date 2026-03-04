using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Trim.Models;

namespace Trim.DbContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSety (EN)
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Lead> Leads => Set<Lead>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<OrderVehicleConfiguration> OrderVehicleConfigurations => Set<OrderVehicleConfiguration>();
    public DbSet<OfferVehicleConfiguration> OfferVehicleConfigurations => Set<OfferVehicleConfiguration>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PdfDocument> PdfDocuments => Set<PdfDocument>();
    public DbSet<CustomerCommunication> CustomerCommunications => Set<CustomerCommunication>();

    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<FleetDiscount> FleetDiscounts => Set<FleetDiscount>();
    public DbSet<SalesBonus> SalesBonuses => Set<SalesBonus>();

    public DbSet<SalespersonSales> SalespersonSales => Set<SalespersonSales>();
    public DbSet<OptionPrice> OptionPrices => Set<OptionPrice>();
    //Vehicle Components
    public DbSet<VehicleCabSize> VehicleCabSizes => Set<VehicleCabSize>();
    public DbSet<VehicleEngine> VehicleEngines => Set<VehicleEngine>();
    public DbSet<VehicleGearbox> VehicleGearboxes => Set<VehicleGearbox>();
    public DbSet<VehicleInterior> VehicleInteriors => Set<VehicleInterior>();
    public DbSet<VehicleDrivetrain> VehicleDrivetrains => Set<VehicleDrivetrain>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ===== Identity inheritance (TPH) =====
        // If ApplicationUser is abstract -> remove HasValue<ApplicationUser>("User")
        b.Entity<ApplicationUser>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Salesperson>("Salesperson")
            .HasValue<SalesAdministrator>("SalesAdministrator")
            .HasValue<Administrator>("Administrator");
        
        // ===== Customers -> Salesperson =====
        b.Entity<Customer>()
            .HasOne(c => c.Salesperson)
            .WithMany() // jeśli nie masz kolekcji po stronie Salesperson
            .HasForeignKey(c => c.SalespersonId)
            .OnDelete(DeleteBehavior.SetNull);

            //v comps
        
                base.OnModelCreating(b);

                b.Entity<VehicleConfiguration>(e =>
                {
                    e.HasKey(x => x.Id);

                    e.HasOne(x => x.Size).WithMany().HasForeignKey(x => x.SizeId).OnDelete(DeleteBehavior.Restrict);
                    e.HasOne(x => x.Engine).WithMany().HasForeignKey(x => x.EngineId).OnDelete(DeleteBehavior.Restrict);
                    e.HasOne(x => x.Gearbox).WithMany().HasForeignKey(x => x.GearboxId).OnDelete(DeleteBehavior.Restrict);
                    e.HasOne(x => x.Interior).WithMany().HasForeignKey(x => x.InteriorId).OnDelete(DeleteBehavior.Restrict);
                    e.HasOne(x => x.Drivetrain).WithMany().HasForeignKey(x => x.DrivetrainId).OnDelete(DeleteBehavior.Restrict);

                    e.Property(x => x.Price).HasPrecision(18,2);
                    e.Property(x => x.Bonus).HasPrecision(18,2);
                    e.Property(x => x.BonusMultiplier).HasPrecision(18,2);
                    e.Property(x => x.AdditionalPrice).HasPrecision(18,2);
                    e.Property(x => x.AdditionalEquipment).HasMaxLength(500);
                });
        
        b.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        b.Entity<OfferVehicle>(entity =>
        {
            entity.HasKey(x => new { x.OfferId, x.VehicleId });

            entity.HasOne(x => x.Vehicle)
                .WithMany(v => v.OfferVehicles)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict); 
            // Restrict jest często lepsze: nie chcesz skasować pojazdu przez skasowanie oferty.
        });
        b.Entity<Offer>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Offers)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<Offer>()
            .HasMany(o => o.OfferVehicleConfigurations)
            .WithOne(vc => vc.Offer)
            .HasForeignKey(vc => vc.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Offer>()
            .HasOne(o => o.Salesperson)
            .WithMany(s => s.CreatedOffers)
            .HasForeignKey(o => o.SalespersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Offer -> Order (0..1)
        b.Entity<Offer>()
            .HasOne(o => o.Order)
            .WithOne()
            .HasForeignKey<Offer>(o => o.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Offer -> PdfDocument (0..1) (1:1)
        b.Entity<PdfDocument>()
            .HasOne(d => d.Offer)
            .WithOne(o => o.PdfDocument)
            .HasForeignKey<PdfDocument>(d => d.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
        

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
            .OnDelete(DeleteBehavior.NoAction);

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
        b.Entity<OptionPrice>()
            .HasIndex(x => new { x.Group, x.EnumValue })
            .IsUnique();
    }
}

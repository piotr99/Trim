using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Trim.Models;

namespace Trim.DbContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSety (EN)
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Order> Orders => Set<Order>();
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
    public DbSet<SalesCase> SalesCases => Set<SalesCase>();

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


        // 1. Oferta -> Pojazdy (1:N)
        b.Entity<Vehicle>()
            .HasOne(v => v.Offer)
            .WithMany(o => o.Vehicles)
            .HasForeignKey(v => v.OfferId)
            .OnDelete(DeleteBehavior.SetNull); // Usunięcie oferty nie usuwa aut, tylko odpina powiązanie

        // 2. Zamówienie -> Pojazdy (1:N)
        b.Entity<Vehicle>()
            .HasOne(v => v.Order)
            .WithMany(o => o.Vehicles)
            .HasForeignKey(v => v.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // 3. Klient -> Pojazdy (1:N)
        b.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId)
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

        b.Entity<SalesCase>()
        .HasOne(sc => sc.Offer)
        .WithOne(o => o.SalesCase)
        .HasForeignKey<Offer>(o => o.SalesCaseId) // Klucz obcy znajduje się w tabeli Offers
        .OnDelete(DeleteBehavior.Restrict);

        // Relacja 1:1 - Zgłoszenie -> Zamówienie
        b.Entity<SalesCase>()
            .HasOne(sc => sc.Order)
            .WithOne(o => o.SalesCase)
            .HasForeignKey<Order>(o => o.SalesCaseId) // Klucz obcy znajduje się w tabeli Orders
            .OnDelete(DeleteBehavior.Restrict);

        // Relacja 1:N - Zgłoszenie -> Komunikacja (zostaje bez zmian)
        b.Entity<CustomerCommunication>()
            .HasOne(c => c.SalesCase)
            .WithMany(sc => sc.ActivityLogs)
            .HasForeignKey(c => c.SalesCaseId)
            .OnDelete(DeleteBehavior.Cascade);




        b.Entity<Order>()
            .Property(o => o.FinalPrice)
            .HasPrecision(18, 2);

        b.Entity<Offer>()
            .Property(o => o.FinalPrice)
            .HasPrecision(18, 2);

        b.Entity<Offer>()
            .Property(o => o.Price)
            .HasPrecision(18, 2);

        b.Entity<Offer>()
            .Property(o => o.Discount)
            .HasPrecision(18, 2);

        b.Entity<Offer>()
            .Property(o => o.Bonus)
            .HasPrecision(18, 2);

        b.Entity<OptionPrice>()
            .Property(o => o.Price)
            .HasPrecision(18, 2);
    }
}

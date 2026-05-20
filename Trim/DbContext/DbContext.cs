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
        // 1. Zawsze tylko JEDNO wywołanie na samej górze
        base.OnModelCreating(b);

        // ===== Identity inheritance (TPH) =====
        b.Entity<ApplicationUser>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Salesperson>("Salesperson")
            .HasValue<SalesAdministrator>("SalesAdministrator")
            .HasValue<Administrator>("Administrator")
            .HasValue<Customer>("Customer"); // <-- BRAKOWAŁO TEGO WPISU!

        // ===== Customers -> Salesperson =====
        b.Entity<Customer>()
            .HasOne(c => c.Salesperson)
            .WithMany()
            .HasForeignKey(c => c.SalespersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Vehicle Components =====
        b.Entity<VehicleConfiguration>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Size).WithMany().HasForeignKey(x => x.SizeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Engine).WithMany().HasForeignKey(x => x.EngineId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Gearbox).WithMany().HasForeignKey(x => x.GearboxId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Interior).WithMany().HasForeignKey(x => x.InteriorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Drivetrain).WithMany().HasForeignKey(x => x.DrivetrainId).OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Bonus).HasPrecision(18, 2);
            e.Property(x => x.BonusMultiplier).HasPrecision(18, 2);
            e.Property(x => x.AdditionalPrice).HasPrecision(18, 2);
            e.Property(x => x.AdditionalEquipment).HasMaxLength(500);
        });

        // ===== Vehicle Relations =====
        // Klient -> Pojazdy (1:N)
        b.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Oferta -> Pojazdy (1:N)
        b.Entity<Vehicle>()
            .HasOne(v => v.Offer)
            .WithMany(o => o.Vehicles)
            .HasForeignKey(v => v.OfferId)
            .OnDelete(DeleteBehavior.SetNull);

        // Zamówienie -> Pojazdy (1:N)
        b.Entity<Vehicle>()
            .HasOne(v => v.Order)
            .WithMany(o => o.Vehicles)
            .HasForeignKey(v => v.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== PriceList -> PriceListItems (1..*) =====
        b.Entity<PriceList>()
            .HasMany(p => p.Items)
            .WithOne(i => i.PriceList)
            .HasForeignKey(i => i.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<PriceListItem>()
            .HasIndex(x => new { x.PriceListId, x.ProductCode })
            .IsUnique();

        // ===== SalespersonSales (Tabela łącząca) =====
        b.Entity<SalespersonSales>()
            .HasKey(x => new { x.SalespersonId, x.VehicleId });

        // NOWE: Blokada kaskady od strony Salesperson, aby uniknąć błędu 1785
        b.Entity<SalespersonSales>()
            .HasOne(x => x.Salesperson) // <-- Wskazujemy konkretną właściwość z Twojej klasy
            .WithMany()
            .HasForeignKey(x => x.SalespersonId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<SalespersonSales>()
            .HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Inne =====
        b.Entity<OptionPrice>()
            .HasIndex(x => new { x.Group, x.EnumValue })
            .IsUnique();

        b.Entity<SalesCase>()
             .HasOne(sc => sc.AssignedSalesperson)
             .WithMany()
             .HasForeignKey(sc => sc.AssignedSalespersonId)
             .OnDelete(DeleteBehavior.Restrict);

        b.Entity<SalesCase>()
            .HasOne(sc => sc.Customer)
            .WithMany()
            .HasForeignKey(sc => sc.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacja 1:1 - Zgłoszenie -> Oferta
        b.Entity<SalesCase>()
            .HasOne(sc => sc.Offer)
            .WithOne(o => o.SalesCase)
            .HasForeignKey<Offer>(o => o.SalesCaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacja 1:1 - Zgłoszenie -> Zamówienie
        b.Entity<SalesCase>()
            .HasOne(sc => sc.Order)
            .WithOne(o => o.SalesCase)
            .HasForeignKey<Order>(o => o.SalesCaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacja 1:N - Zgłoszenie -> Komunikacja
        b.Entity<CustomerCommunication>()
            .HasOne(c => c.SalesCase)
            .WithMany(sc => sc.ActivityLogs)
            .HasForeignKey(c => c.SalesCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        //
        b.Entity<Customer>()
            .HasMany(c => c.SalesCases)
            .WithOne(cs => cs.Customer)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Precyzja walutowa =====
        b.Entity<Order>().Property(o => o.FinalPrice).HasPrecision(18, 2);
        b.Entity<Offer>().Property(o => o.FinalPrice).HasPrecision(18, 2);
        b.Entity<Offer>().Property(o => o.Price).HasPrecision(18, 2);
        b.Entity<Offer>().Property(o => o.Discount).HasPrecision(18, 2);
        b.Entity<Offer>().Property(o => o.Bonus).HasPrecision(18, 2);
        b.Entity<OptionPrice>().Property(o => o.Price).HasPrecision(18, 2);
    }
}

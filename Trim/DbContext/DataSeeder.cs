using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trim.Models;

namespace Trim.DbContext;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Migracje
        await db.Database.MigrateAsync();

        // ===== Roles =====
        var roles = new[] { "Salesperson", "SalesAdministrator", "Administrator" , "Customer" };
        foreach (var role in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        // ===== Users (2 + 2) =====
        async Task<ApplicationUser> EnsureUserAsync(ApplicationUser user, string password, string role)
        {
            var existing = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);
            if (existing != null)
            {
                if (!await userManager.IsInRoleAsync(existing, role))
                    await userManager.AddToRoleAsync(existing, role);

                return existing;
            }

            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Nie udało się utworzyć usera: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        if (await db.Orders.AnyAsync())
            return;

        var admin1 = await EnsureUserAsync(
            new Administrator
            {
                UserName = "admin@test.pl",
                Email = "admin@test.pl",
                FirstName = "Admin",
                LastName = "admin"
            },
            "Admin1!", "Administrator");

        var salesperson1 = (Salesperson)await EnsureUserAsync(
            new Salesperson
            {
                UserName = "handlowiec1@demo.local",
                Email = "handlowiec1@demo.local",
                PhoneNumber = "+48111111111",
                FirstName = "Jan",
                LastName = "Kowalski"
            },
            "Demo!12345",
            "Salesperson");

        var salesperson2 = (Salesperson)await EnsureUserAsync(
            new Salesperson
            {
                UserName = "handlowiec2@demo.local",
                Email = "handlowiec2@demo.local",
                PhoneNumber = "+48222222222",
                FirstName = "Anna",
                LastName = "Nowak"
            },
            "Demo!12345",
            "Salesperson");

        _ = await EnsureUserAsync(
            new SalesAdministrator
            {
                UserName = "adminsprz1@demo.local",
                Email = "adminsprz1@demo.local",
                PhoneNumber = "+48333333333",
                FirstName = "Piotr",
                LastName = "Zieliński"
            },
            "Demo!12345",
            "SalesAdministrator");

        _ = await EnsureUserAsync(
            new SalesAdministrator
            {
                UserName = "adminsprz2@demo.local",
                Email = "adminsprz2@demo.local",
                PhoneNumber = "+48444444444",
                FirstName = "Ewa",
                LastName = "Wiśniewska"
            },
            "Demo!12345",
            "SalesAdministrator");

        // ===== Customer (2) =====
        var customer1 = await EnsureUserAsync(new Customer
        {
            UserName = "marek.nowicki@demo.local",
            FirstName = "Marek",
            LastName = "Nowicki",
            Email = "marek.nowicki@demo.local",
            PhoneNumber = "+48555555555",
            CompanyName = "Transport Demo Sp. z o.o.",
            Address = "Warszawa, ul. Przykładowa 1",
            SalespersonId = salesperson1.Id,
            TaxId = "123qwe"
        },
        "Daemo!12345",
        "Customer");

        var customer2 = await EnsureUserAsync(new Customer
        {
            UserName = "karolina.mazur@demo.local",
            FirstName = "Karolina",
            LastName = "Mazur",
            Email = "karolina.mazur@demo.local",
            PhoneNumber = "+48666666666",
            CompanyName = "Logistyka Test S.A.",
            Address = "Kraków, ul. Testowa 2",
            SalespersonId = salesperson2.Id,
            TaxId = "456asd"
        },
        "Daemo!12345",
        "Customer");

        await db.SaveChangesAsync();

        // ===== Sales Cases (Zgłoszenia) =====
        var case1 = new SalesCase
        {
            CaseNumber = "CASE-2026-0001",
            Title = "Zapytanie o Scanię S600 z silnikiem 13L",
            Description = "Klient pytał o możliwość szybkiej dostawy i finansowanie leasingowe.",
            Status = SalesCaseStatusEnum.NEGOTIATION,
            CustomerId = customer1.Id,
            AssignedSalespersonId = salesperson1.Id,
        };

        var case2 = new SalesCase
        {
            CaseNumber = "CASE-2026-0002",
            Title = "Rozbudowa floty - model R480",
            Description = "Wymiana starych aut na nowe. Potrzebne podwozie 6x2.",
            Status = SalesCaseStatusEnum.NEW,
            CustomerId = customer2.Id,
            AssignedSalespersonId = salesperson2.Id
        };

        var case3 = new SalesCase
        {
            CaseNumber = "CASE-2026-0003",
            Title = "Rozbudowa floty - model R420",
            Description = "Potrzebne podwozie 60x12",
            Status = SalesCaseStatusEnum.NEW,
            CustomerId = customer1.Id,
            AssignedSalespersonId = salesperson1.Id
        };

        db.SalesCases.AddRange(case1, case2, case3);

        await db.SaveChangesAsync();
        // ===== Vehicle Components =====

        //Cab
        if (!db.VehicleCabSizes.Any())
        {
            db.VehicleCabSizes.AddRange(
                new VehicleCabSize { Name = "P - niska (P-cab)", Price = 44000m, BonusMultiplier = 0.4m },
                new VehicleCabSize { Name = "G - normalna (G-cab)", Price = 40000m, BonusMultiplier = 0.3m },
                new VehicleCabSize { Name = "R - wysoka (R-cab)", Price = 48000m },
                new VehicleCabSize { Name = "S - najwyższa (S-cab)", Price = 60000m },
                new VehicleCabSize { Name = "L - długa (L-cab)", Price = 42000m }
            );
        }
        // ENGINE
        if (!db.VehicleEngines.Any())
        {
            db.VehicleEngines.AddRange(
                new VehicleEngine { Name = "9L 360 KM", Price = 38000m, BonusMultiplier = 2.5m },
                new VehicleEngine { Name = "13L 560 KM", Price = 40000m },
                new VehicleEngine { Name = "16L V8 770 KM", Price = 60000m }
            );
        }
        // GEARBOX
        if (!db.VehicleGearboxes.Any())
        {
            db.VehicleGearboxes.AddRange(
                    new VehicleGearbox { Name = "Manual (G-series)", Price = 60000m },
                    new VehicleGearbox { Name = "Opticruise / Automated (AMT)", Price = 100000m, BonusMultiplier = 0.1m },
                    new VehicleGearbox { Name = "Allison (automat - zastosowania specjalne)", Price = 100000m }
            );
        }
        // INTERIOR
        if (!db.VehicleInteriors.Any())
        {
            db.VehicleInteriors.AddRange(
                    new VehicleInterior { Name = "Standard", Price = 60000m },
                    new VehicleInterior { Name = "Comfort", Price = 100000m, BonusMultiplier = 0.8m },
                    new VehicleInterior { Name = "Premium / Highline", Price = 160000m },
                    new VehicleInterior { Name = "Luxury / Topline", Price = 260000m }
                );
        }
        // DRIVETRAIN
        if (!db.VehicleDrivetrains.Any())
        {
            db.VehicleDrivetrains.AddRange(
                new VehicleDrivetrain { Name = "4x2", Price = 80000m },
                new VehicleDrivetrain { Name = "6x2", Price = 90000m },
                new VehicleDrivetrain { Name = "6x4", Price = 100000m },
                new VehicleDrivetrain { Name = "8x2", Price = 100000m, BonusMultiplier = 0.5m },
                new VehicleDrivetrain { Name = "8x4", Price = 160000m }
                );
        }

        // Zapisujemy komponenty, aby pobrać ich wygenerowane ID dla ofert.
        // Usunięto db.ChangeTracker.Clear(); aby nie gubić referencji.
        await db.SaveChangesAsync();

        var gCab = await db.VehicleCabSizes.FirstOrDefaultAsync(x => x.Name.Contains("G - normalna"));
        var eng13 = await db.VehicleEngines.FirstOrDefaultAsync(x => x.Name.Contains("13L 560"));
        var gbAllison = await db.VehicleGearboxes.FirstOrDefaultAsync(x => x.Name.Contains("Allison"));
        var intrComfort = await db.VehicleInteriors.FirstOrDefaultAsync(x => x.Name.Contains("Comfort"));
        var drv6x2 = await db.VehicleDrivetrains.FirstOrDefaultAsync(x => x.Name == "6x2");

        var config1 = new VehicleConfiguration
        {
            SizeId = gCab.Id,
            EngineId = eng13.Id,
            GearboxId = gbAllison.Id,
            InteriorId = intrComfort.Id,
            DrivetrainId = drv6x2.Id
        };

        var config2 = new VehicleConfiguration
        {
            SizeId = gCab.Id,
            EngineId = eng13.Id,
            GearboxId = gbAllison.Id,
            InteriorId = intrComfort.Id,
            DrivetrainId = drv6x2.Id
        };

        var config3 = new VehicleConfiguration
        {
            SizeId = gCab.Id,
            EngineId = eng13.Id,
            GearboxId = gbAllison.Id,
            InteriorId = intrComfort.Id,
            DrivetrainId = drv6x2.Id
        };

        var config4 = new VehicleConfiguration
        {
            SizeId = gCab.Id,
            EngineId = eng13.Id,
            GearboxId = gbAllison.Id,
            InteriorId = intrComfort.Id,
            DrivetrainId = drv6x2.Id
        };

        var config5 = new VehicleConfiguration
        {
            SizeId = gCab.Id,
            EngineId = eng13.Id,
            GearboxId = gbAllison.Id,
            InteriorId = intrComfort.Id,
            DrivetrainId = drv6x2.Id
        };

        // ===== POJAZDY =====
        var vehicle1 = new Vehicle
        {
            Name = "Scania S600",
            Vin = "VINDEMO00000000001",
            Status = VehicleStatusEnum.AVAILABLE,
            CustomerId = customer1.Id, // Pojazd wie, do kogo należy
            Configuration = config1    // Przypinamy konfigurację prosto do auta
        };

        var vehicle2 = new Vehicle
        {
            Name = "Scania R480",
            Vin = "VINDEMO00000000002",
            Status = VehicleStatusEnum.ORDERED,
            CustomerId = customer2.Id,
            Configuration = config2
        };

        var vehicle3 = new Vehicle
        {
            Name = "Scania R420",
            Vin = "VINDEMO00000000003",
            Status = VehicleStatusEnum.DRAFT,
            Configuration = config3
        };

        var vehicle4 = new Vehicle
        {
            Name = "Scania S500 Highline",
            Vin = "VINDEMO00000000004",
            Status = VehicleStatusEnum.DRAFT,
            Configuration = config4,
            ParkingLot = true
        };

        // Utworzenie pojazdu nr 5 (np. z config5)
        var vehicle5 = new Vehicle
        {
            Name = "Scania P320 XT",
            Vin = "VINDEMO00000000005",
            Status = VehicleStatusEnum.DRAFT,
            Configuration = config5,
            ParkingLot = true
        };

        // Zapiszmy je najpierw, aby EF poprawnie śledził graf obiektów
        db.Vehicles.AddRange(vehicle1, vehicle2, vehicle3, vehicle4, vehicle5);
        await db.SaveChangesAsync();

        // ===== OFERTY (Offers) =====
        var offer1 = new Offer
        {
            OfferFriendlyName = "OFF-2026-0001",
            Status = OfferStatusEnum.IN_NEGOTIATION,
            SalesCaseId = case1.Id,
            Vehicles = new List<Vehicle> { vehicle1 }
        };

        var offer2 = new Offer
        {
            OfferFriendlyName = "OFF-2026-0002",
            Status = OfferStatusEnum.DRAFT,
            SalesCaseId = case2.Id,
            Vehicles = new List<Vehicle> { vehicle2 }
        };

        var offer3 = new Offer
        {
            OfferFriendlyName = "OFF-2026-0003",
            Status = OfferStatusEnum.IN_NEGOTIATION,
            SalesCaseId = case3.Id,
            Vehicles = new List<Vehicle> { vehicle3 }
        };

        db.Offers.AddRange(offer1, offer2, offer3);
        await db.SaveChangesAsync();

        // ===== ZAMÓWIENIA (Orders) =====
        var order1 = new Order
        {
            OrderNumber = "ORD-2026-0001",
            Status = OrderStatusEnum.IN_PROGRESS,
            FinalPrice = 505000m,
            SalesCaseId = case1.Id,
            Vehicles = new List<Vehicle> { vehicle1 } // To samo auto ląduje w zamówieniu
        };

        var order2 = new Order
        {
            OrderNumber = "ORD-2026-0002",
            Status = OrderStatusEnum.NEW,
            FinalPrice = 470000m,
            SalesCaseId = case2.Id,
            Vehicles = new List<Vehicle> { vehicle2 }
        };

        db.Orders.AddRange(order1, order2);
        await db.SaveChangesAsync();

        // ===== CustomerCommunications (2) =====
        db.CustomerCommunications.AddRange(
            new CustomerCommunication
            {
                IsPrivateMessage = false,
                MessageContent = "cokolwiek",
                SentAt = DateTime.UtcNow.AddDays(-2),
                DeliveryStatus = "DELIVERED",
                SalesCase = case1
            },
            new CustomerCommunication
            {
                IsPrivateMessage = false,
                MessageContent = "cokolwiek ale 2",
                SentAt = DateTime.UtcNow.AddDays(-1),
                DeliveryStatus = "SENT",
                SalesCase = case2
            }
        );

        // ===== PriceLists (2) =====
        var priceList1 = new PriceList
        {
            Name = "Price List 2026 Q1",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 3, 31)
        };

        var priceList2 = new PriceList
        {
            Name = "Price List 2026 Q2",
            ValidFrom = new DateTime(2026, 4, 1),
            ValidTo = new DateTime(2026, 6, 30)
        };

        db.PriceLists.AddRange(priceList1, priceList2);

        // ===== PriceListItems (2) =====
        db.PriceListItems.AddRange(
            new PriceListItem { PriceList = priceList1, ProductCode = "R500-BASE", BasePrice = 500000m },
            new PriceListItem { PriceList = priceList2, ProductCode = "S450-BASE", BasePrice = 470000m }
        );

        // ===== FleetDiscounts (2) =====
        db.FleetDiscounts.AddRange(
            new FleetDiscount { QuantityThreshold = 5, DiscountPercent = 2.50m },
            new FleetDiscount { QuantityThreshold = 10, DiscountPercent = 5.00m }
        );

        // ===== SalesBonuses (2) =====
        db.SalesBonuses.AddRange(
            new SalesBonus
            { Name = "Quarterly bonus", Percent = 1.50m, Conditions = "Min. 3 vehicles sold per quarter" },
            new SalesBonus { Name = "Premium bonus", Percent = 2.50m, Conditions = "Average margin > 4%" }
        );

        // ===== SalespersonSales (2) =====
        db.SalespersonSales.AddRange(
            new SalespersonSales { SalespersonId = salesperson1.Id, Vehicle = vehicle1, Quantity = 1 },
            new SalespersonSales { SalespersonId = salesperson2.Id, Vehicle = vehicle2, Quantity = 2 }
        );

        // Zapis końcowy całego grafu obiektów
        await db.SaveChangesAsync();
    }
}
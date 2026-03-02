using System;
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
        await db.Database.EnsureCreatedAsync();
        await db.Database.MigrateAsync();


        // ===== Roles =====
        var roles = new[] { "Salesperson", "SalesAdministrator", "Administrator" };
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
        var customer1 = new Customer
        {
            FirstName = "Marek",
            LastName = "Nowicki",
            Email = "marek.nowicki@demo.local",
            Phone = "+48555555555",
            CompanyName = "Transport Demo Sp. z o.o.",
            Address = "Warszawa, ul. Przykładowa 1",
            SalespersonId = salesperson1.Id,
            TaxId = "123qwe"
        };

        var customer2 = new Customer
        {
            FirstName = "Karolina",
            LastName = "Mazur",
            Email = "karolina.mazur@demo.local",
            Phone = "+48666666666",
            CompanyName = "Logistyka Test S.A.",
            Address = "Kraków, ul. Testowa 2",
            SalespersonId = salesperson2.Id,
            TaxId = "456asd"
        };

        // ===== Leads (2) =====
        var lead1 = new Lead
        {
            CompanyName = "New Company 1",
            TaxId = "1111111111",
            ContactEmail = "kontakt1@demo.local",
            ContactPhone = "+48777777777",
            Status = LeadStatusEnum.NEW,
        };

        var lead2 = new Lead
        {
            CompanyName = "New Company 2",
            TaxId = "2222222222",
            ContactEmail = "kontakt2@demo.local",
            ContactPhone = "+48888888888",
            Status = LeadStatusEnum.IN_PROGRESS,
        };
        // ===== Vehicle Components =====
        //Cab
        if (!db.VehicleCabSizes.Any(x => x.Id == 0))
        {
            db.VehicleCabSizes.AddRange(
                new VehicleCabSize { Id = 0, Name = "P - niska (P-cab)", Price = 44000m },
                new VehicleCabSize { Id = 1, Name = "G - normalna (G-cab)", Price = 40000m },
                new VehicleCabSize { Id = 2, Name = "R - wysoka (R-cab)", Price = 48000m },
                new VehicleCabSize { Id = 3, Name = "S - najwyższa (S-cab)", Price = 60000m },
                new VehicleCabSize { Id = 4, Name = "L - długa (L-cab)", Price = 42000m }
            );
        }
        // ENGINE
        if (!db.VehicleEngines.Any(x => x.Id == 1))
        {
            db.VehicleEngines.AddRange(
                new VehicleEngine { Id = 0, Name = "9L 360 KM", Price = 38000m },
                new VehicleEngine { Id = 1, Name = "13L 560 KM", Price = 40000m },
                new VehicleEngine { Id = 2, Name = "16L V8 770 KM", Price = 60000m }
            );
        }
        // GEARBOX
        if (!db.VehicleGearboxes.Any())
        {
            db.VehicleGearboxes.AddRange(
                    new VehicleGearbox { Id = 0, Name = "Manual (G-series)",Price = 60000m },
                    new VehicleGearbox { Id = 1, Name = "Opticruise / Automated (AMT)",Price = 100000m },
                    new VehicleGearbox { Id = 2, Name = "Allison (automat - zastosowania specjalne)", Price = 100000m }
            );
        }
        // INTERIOR
        if (!db.VehicleInteriors.Any())
        {
            db.VehicleInteriors.AddRange(
                    new VehicleInterior { Id = 0, Name = "Standard",              Price = 60000m },
                    new VehicleInterior { Id = 1, Name = "Comfort",               Price = 100000m },
                    new VehicleInterior { Id = 2, Name = "Premium / Highline",    Price = 160000m },
                    new VehicleInterior { Id = 3, Name = "Luxury / Topline",      Price = 260000m }
                );
        }
        // DRIVETRAIN
        if (!db.VehicleDrivetrains.Any())
        {
            db.VehicleDrivetrains.AddRange(
                new VehicleDrivetrain { Id = 0, Name = "4x2", Price = 80000m },
                new VehicleDrivetrain { Id = 1, Name = "6x2", Price = 90000m },
                new VehicleDrivetrain { Id = 2, Name = "6x4", Price = 100000m} ,
                new VehicleDrivetrain { Id = 3, Name = "8x2", Price = 100000m },
                new VehicleDrivetrain { Id = 4, Name = "8x4", Price = 160000m }
                );
        }
        
        await db.SaveChangesAsync();
        
        // ===== Vehicles (2) =====
        
        var vehicle1 = new Vehicle
        {
            Name = "Scania S600",
            Vin = "VINDEMO00000000001",
            Status = VehicleStatusEnum.AVAILABLE
        };

        var vehicle2 = new Vehicle
        {
            Name = "Scania R480",
            Vin = "VINDEMO00000000002",
            Status = VehicleStatusEnum.ORDERED
        };

        // Podepnij pojazdy do klientów
        customer1.Vehicles ??= new List<Vehicle>();
        customer2.Vehicles ??= new List<Vehicle>();
        customer1.Vehicles.Add(vehicle1);
        customer2.Vehicles.Add(vehicle2);

        // Dodaj do contextu
        db.Customers.AddRange(customer1, customer2);
        db.Leads.AddRange(lead1, lead2);

        Console.WriteLine(db.VehicleCabSizes.Count()); // otrzymuję 0
        // ===== Offers (2) =====
        var offer1 = new Offer
        {
            OfferFriendlyName = "OFF-2026-0001",
            Status = OfferStatusEnum.IN_NEGOTIATION,
            Customer = customer1,
            Salesperson = salesperson1,
            OfferVehicleConfigurations = new List<OfferVehicleConfiguration>
            {
                new OfferVehicleConfiguration
                {
                    Size = db.VehicleCabSizes.Single(x => x.Id == 0),
                    Engine = db.VehicleEngines.Single(x => x.Id == 1),
                    Gearbox = db.VehicleGearboxes.Single(x => x.Id == 2),
                    Interior = db.VehicleInteriors.Single(x => x.Id == 1),
                    Drivetrain = db.VehicleDrivetrains.Single(x => x.Id == 0),
                }
            }
        };

        var offer2 = new Offer
        {
            OfferFriendlyName = "OFF-2026-0002",
            Status = OfferStatusEnum.DRAFT,
            Customer = customer2,
            Salesperson = salesperson2,
            OfferVehicleConfigurations = new List<OfferVehicleConfiguration>
            {
                new OfferVehicleConfiguration
                {
                    Size = db.VehicleCabSizes.First(x => x.Id == 0),
                    Engine = db.VehicleEngines.First(x => x.Id == 1),
                    Gearbox = db.VehicleGearboxes.First(x => x.Id == 2),
                    Interior = db.VehicleInteriors.First(x => x.Id == 1),
                    Drivetrain = db.VehicleDrivetrains.First(x => x.Id == 0),
                }
            }
        };

        db.Offers.AddRange(offer1, offer2);

        // ===== OfferVersions (2) =====
        db.OfferVersions.AddRange(
            new OfferVersion
            {
                Offer = offer1,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Price = 520000m,
                FinalPrice = 520000m
            },
            new OfferVersion
            {
                Offer = offer2,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Price = 480000m,
                FinalPrice = 480000m
            }
        );

        // ===== Orders (2) =====
        var order1 = new Order
        {
            OrderNumber = "ORD-2026-0001",
            Status = OrderStatusEnum.IN_PROGRESS,
            FinalPrice = 505000m,
            Customer = customer1,
            Vehicle = vehicle1,
            SalespersonId = salesperson1.Id
        };

        var order2 = new Order
        {
            OrderNumber = "ORD-2026-0002",
            Status = OrderStatusEnum.NEW,
            FinalPrice = 470000m,
            Customer = customer2,
            Vehicle = vehicle2,
            SalespersonId = salesperson2.Id
        };

        db.Orders.AddRange(order1, order2);

        // Link offers -> orders (0..1)
        offer1.Order = order1;
        offer2.Order = order2;

        // ===== Invoices (2) =====
        db.Invoices.AddRange(
            new Invoice
            {
                InvoiceNumber = "INV-2026-0001",
                SaleDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(13),
                GrossAmount = 621150m,
                Status = InvoiceStatusEnum.UNPAID,
                Order = order1
            },
            new Invoice
            {
                InvoiceNumber = "INV-2026-0002",
                SaleDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                GrossAmount = 578100m,
                Status = InvoiceStatusEnum.PARTIALLY_PAID,
                Order = order2
            }
        );

        // ===== PdfDocuments (2) =====
        db.PdfDocuments.AddRange(
            new PdfDocument
            {
                Offer = offer1,
                FilePath = "pdf/offers/OFF-2026-0001.pdf",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new PdfDocument
            {
                Offer = offer2,
                FilePath = "pdf/offers/OFF-2026-0002.pdf",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        );

        // ===== CustomerCommunications (2) =====
        db.CustomerCommunications.AddRange(
            new CustomerCommunication
            {
                Type = MessageTypeEnum.OFFER_SENT,
                SentAt = DateTime.UtcNow.AddDays(-2),
                DeliveryStatus = "DELIVERED",
                Offer = offer1,
                Customer = customer1
            },
            new CustomerCommunication
            {
                Type = MessageTypeEnum.PAYMENT_REMINDER,
                SentAt = DateTime.UtcNow.AddDays(-1),
                DeliveryStatus = "SENT",
                Invoice = db.Invoices.Local.First(i => i.InvoiceNumber == "INV-2026-0001"),
                Customer = customer1
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

        // 5) Zapis na końcu (jeden SaveChanges)
        await db.SaveChangesAsync();

        //Vehicle Options
        var existing = await db.OptionPrices
            .Select(x => new { x.Group, x.EnumValue })
            .ToListAsync();
        var existingSet = existing
            .Select(x => (x.Group, x.EnumValue))
            .ToHashSet();
        var toAdd = new List<OptionPrice>();

        void Add(string group, int enumValue, decimal price)
        {
            if (!existingSet.Contains((group, enumValue)))
            {
                toAdd.Add(new OptionPrice
                {
                    Group = group,
                    EnumValue = enumValue,
                    Price = price
                });
            }
        }

        if (!await db.OptionPrices.AnyAsync())
        {
            db.OptionPrices.AddRange(
                //engine
                new OptionPrice { Group="Engine", EnumValue=(int)VehicleEngineEnum.L09, Price=38000m },
                new OptionPrice { Group="Engine", EnumValue=(int)VehicleEngineEnum.L13, Price=40000m },
                new OptionPrice { Group="Engine", EnumValue=(int)VehicleEngineEnum.V8_16, Price=60000m },
                //cab
                new OptionPrice { Group="CabSize", EnumValue=(int)VehicleCabSizeEnum.GCab, Price=40000m },
                new OptionPrice { Group="CabSize", EnumValue=(int)VehicleCabSizeEnum.LCab, Price=42000m },
                new OptionPrice { Group="CabSize", EnumValue=(int)VehicleCabSizeEnum.PCab, Price=44000m },
                new OptionPrice { Group="CabSize", EnumValue=(int)VehicleCabSizeEnum.RCab, Price=48000m },
                new OptionPrice { Group="CabSize", EnumValue=(int)VehicleCabSizeEnum.SCab, Price=60000m },
                //gearbox
                new OptionPrice { Group="Gearbox", EnumValue=(int)VehicleGearboxEnum.AllisonAutomatic, Price=100000m },
                new OptionPrice { Group="Gearbox", EnumValue=(int)VehicleGearboxEnum.Manual, Price=60000m },
                new OptionPrice { Group="Gearbox", EnumValue=(int)VehicleGearboxEnum.Opticruise, Price=100000m },
                //interior
                new OptionPrice { Group="Interior", EnumValue=(int)VehicleInteriorEnum.Standard, Price=60000m },
                new OptionPrice { Group="Interior", EnumValue=(int)VehicleInteriorEnum.Comfort, Price=100000m },
                new OptionPrice { Group="Interior", EnumValue=(int)VehicleInteriorEnum.Luxury, Price=260000m },
                new OptionPrice { Group="Interior", EnumValue=(int)VehicleInteriorEnum.Premium, Price=160000m },
                //drivetrain
                new OptionPrice { Group="Drivetrain", EnumValue=(int)VehicleDrivetrainEnum._4x2, Price=80000m },
                new OptionPrice { Group="Drivetrain", EnumValue=(int)VehicleDrivetrainEnum._6x2, Price=90000m },
                new OptionPrice { Group="Drivetrain", EnumValue=(int)VehicleDrivetrainEnum._6x4, Price=100000m },
                new OptionPrice { Group="Drivetrain", EnumValue=(int)VehicleDrivetrainEnum._8x2, Price=100000m },
                new OptionPrice { Group="Drivetrain", EnumValue=(int)VehicleDrivetrainEnum._8x4, Price=160000m }
            );
        }

        await db.SaveChangesAsync();
    
}
}

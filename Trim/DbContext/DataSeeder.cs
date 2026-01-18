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
        await db.Database.MigrateAsync();
        

        // ===== Roles =====
        var roles = new[] { "Salesperson", "SalesAdministrator", "Administrator"};
        foreach (var role in roles)
        {
            var  roleExists = await roleManager.RoleExistsAsync(role);
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
        
        
        var admin1 = await EnsureUserAsync(
            new Administrator
            {
                UserName = "admin@test.pl",
                Email = "admin@test.pl",  
                FirstName = "Admin",
                LastName = "admin"
            },
            "Admin1!","Administrator");
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

        // Jeśli już coś jest w bazie domenowej – nie seedujemy ponownie
        if (await db.TransportCompanies.AnyAsync())
            return;

        // ===== TransportCompany (2) =====
        var company1 = new TransportCompany
        {
            Name = "Transport Demo Sp. z o.o.",
            TaxId = "1234567890",
            Address = "Warszawa, ul. Przykładowa 1",
            
        };

        var company2 = new TransportCompany
        {
            Name = "Logistyka Test S.A.",
            TaxId = "9876543210",
            Address = "Kraków, ul. Testowa 2",
        };

        db.TransportCompanies.AddRange(company1, company2);

        // ===== Customer (2) =====
        var customer1 = new Customer
        {
            FirstName = "Marek",
            LastName = "Nowicki",
            Email = "marek.nowicki@demo.local",
            Phone = "+48555555555",
            CompanyName = "Transport Demo Sp. z o.o.",
            Address = "Warszawa, ul. Przykładowa 1",
            SalespersonId = salesperson1.Id
            
        };

        var customer2 = new Customer
        {
            FirstName = "Karolina",
            LastName = "Mazur",
            Email = "karolina.mazur@demo.local",
            Phone = "+48666666666",
            CompanyName = "Logistyka Test S.A.",
            Address = "Kraków, ul. Testowa 2",
            SalespersonId = salesperson2.Id
        };

        // ===== Lead (2) =====
        var lead1 = new Lead
        {
            CompanyName = "New Company 1",
            TaxId = "1111111111",
            ContactEmail = "kontakt1@demo.local",
            ContactPhone = "+48777777777",
            Status = LeadStatusEnum.NEW,
            TransportCompany = company1
        };

        var lead2 = new Lead
        {
            CompanyName = "New Company 2",
            TaxId = "2222222222",
            ContactEmail = "kontakt2@demo.local",
            ContactPhone = "+48888888888",
            Status = LeadStatusEnum.IN_PROGRESS,
            TransportCompany = company2
        };

        db.Leads.AddRange(lead1, lead2);

        // ===== Vehicle (2) =====
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

        
        customer1.Vehicles.Add(vehicle1);
        customer2.Vehicles.Add(vehicle2);
        db.Customers.AddRange(customer1, customer2);
        

        // ===== Option (2) =====
        var option1 = new Option
        {
            Description = "Premium navigation",
            Price = 4500m
        };

        var option2 = new Option
        {
            Description = "LED headlights",
            Price = 3200m
        };

        db.Options.AddRange(option1, option2);
        

        // ===== Offer (2) =====
        var offer1 = new Offer
        {
            OfferFriendlyName = "OFF-2026-0001",
            Status = OfferStatusEnum.IN_NEGOTIATION,
            Customer = customer1,
            SalespersonId = salesperson1.Id,
        };

        offer1.OfferVehicles.Add(new OfferVehicle { Vehicle = vehicle1 });

        var offer2 = new Offer
        {
            OfferFriendlyName = "OFF-2026-0002",
            Status = OfferStatusEnum.DRAFT,
            Customer = customer2,
            SalespersonId = salesperson2.Id,
        };

        offer2.OfferVehicles.Add(new OfferVehicle { Vehicle = vehicle2 });
        db.Offers.AddRange(offer1, offer2);

        // ===== OfferVersion (2) =====
        var version1 = new OfferVersion
        {
            Offer = offer1,
            VersionNumber = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Price = 520000m,
        };

        var version2 = new OfferVersion
        {
            Offer = offer2,
            VersionNumber = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Price = 480000m,
        };

        db.OfferVersions.AddRange(version1, version2);

        // ===== Order (2) =====
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

        // ===== Invoice (2) =====
        var invoice1 = new Invoice
        {
            InvoiceNumber = "INV-2026-0001",
            SaleDate = DateTime.UtcNow.AddDays(-1),
            DueDate = DateTime.UtcNow.AddDays(13),
            GrossAmount = 621150m,
            Status = InvoiceStatusEnum.UNPAID,
            Order = order1
        };

        var invoice2 = new Invoice
        {
            InvoiceNumber = "INV-2026-0002",
            SaleDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            GrossAmount = 578100m,
            Status = InvoiceStatusEnum.PARTIALLY_PAID,
            Order = order2
        };

        db.Invoices.AddRange(invoice1, invoice2);

        // ===== PdfDocument (2) =====
        var pdf1 = new PdfDocument
        {
            Offer = offer1,
            FilePath = "pdf/offers/OFF-2026-0001.pdf",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var pdf2 = new PdfDocument
        {
            Offer = offer2,
            FilePath = "pdf/offers/OFF-2026-0002.pdf",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        db.PdfDocuments.AddRange(pdf1, pdf2);

        // ===== CustomerCommunication (2) =====
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
                Invoice = invoice1,
                Customer = customer1
            }
        );

        // ===== PriceList (2) =====
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

        // ===== PriceListItem (2) =====
        db.PriceListItems.AddRange(
            new PriceListItem { PriceList = priceList1, ProductCode = "R500-BASE", BasePrice = 500000m },
            new PriceListItem { PriceList = priceList2, ProductCode = "S450-BASE", BasePrice = 470000m }
        );

        // ===== FleetDiscount (2) =====
        db.FleetDiscounts.AddRange(
            new FleetDiscount { QuantityThreshold = 5, DiscountPercent = 2.50m },
            new FleetDiscount { QuantityThreshold = 10, DiscountPercent = 5.00m }
        );

        // ===== SalesBonus (2) =====
        db.SalesBonuses.AddRange(
            new SalesBonus { Name = "Quarterly bonus", Percent = 1.50m, Conditions = "Min. 3 vehicles sold per quarter" },
            new SalesBonus { Name = "Premium bonus", Percent = 2.50m, Conditions = "Average margin > 4%" }
        );

        // ===== SalespersonSales (2) =====
        db.SalespersonSales.AddRange(
            new SalespersonSales { SalespersonId = salesperson1.Id, Vehicle = vehicle1, Quantity = 1 },
            new SalespersonSales { SalespersonId = salesperson2.Id, Vehicle = vehicle2, Quantity = 2 }
        );

        await db.SaveChangesAsync();
    }
}

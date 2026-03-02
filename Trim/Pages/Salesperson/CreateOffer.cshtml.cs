using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Trim.Pages.Salesperson.Partial;

public class CreateOffer : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateOffer(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    } 

    [BindProperty(SupportsGet = true)]
    public int customerId { get; set; }   //otrzymuję z asp-route-customerId="@Model.Id"
    
    [BindProperty]
    public Offer NewOffer { get; set; } = new();

    [BindProperty]
    public List<Vehicle> NewVehicles { get; set; } = new() { new Vehicle() };

    [BindProperty]
    public OfferVersion NewOfferVersion { get; set; } = new();
    [BindProperty]
    public List<OfferVehicleConfiguration> NewVehicleConfigurations { get; set; } = new();

    [BindProperty]
    public PdfDocument NewPdfDocument { get; set; } = new();
    //Vehicle components
    public List<VehicleCabSize> VehicleCabSizes { get; private set; } = new();
    
    public List<VehicleEngine> VehicleEngines { get; private set; } = new();
    
    public List<VehicleGearbox> VehicleGearboxes { get; private set; } = new();
    
    public List<VehicleInterior> VehicleInteriors { get; private set; } = new();
    
    public List<VehicleDrivetrain> VehicleDrivetrains { get; private set; } = new();
    
    private async Task LoadVehicleComponentsAsync()
    {
        VehicleCabSizes = await _db.VehicleCabSizes.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        VehicleEngines = await _db.VehicleEngines.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        VehicleGearboxes = await _db.VehicleGearboxes.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        VehicleInteriors = await _db.VehicleInteriors.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        VehicleDrivetrains = await _db.VehicleDrivetrains.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
    }
    
    public async Task OnGet()
    {
        await LoadVehicleComponentsAsync();
        NewVehicleConfigurations ??= new List<OfferVehicleConfiguration>();
        if (NewVehicleConfigurations.Count == 0)
            NewVehicleConfigurations.Add(new OfferVehicleConfiguration());
        NewOffer = new Offer
        {
            OfferFriendlyName = $"OFF-{DateTime.UtcNow:yyyymmdd}-"
        };
    }

    public async Task<IActionResult> OnPostCreateOfferAsync()
    {
        await LoadVehicleComponentsAsync();
        var user = await _userManager.GetUserAsync(User);

        // 1) jeśli zalogowany Salesperson
        if (user != null && await _userManager.IsInRoleAsync(user, "Salesperson"))
        {
            NewOffer.SalespersonId = user.Id;
        }
        else
        {
            // 2) spróbuj wziąć handlowca z klienta
            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer?.SalespersonId != null)
            {
                NewOffer.SalespersonId = customer.SalespersonId.Value;
            }
            else
            {
                // 3) fallback: weź pierwszego handlowca z bazy (albo zwróć błąd)
                var fallbackSalespersonId = await _db.Users
                    .Where(u => EF.Property<string>(u, "UserType") == "Salesperson")
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                if (fallbackSalespersonId == 0)
                    return BadRequest("Brak handlowca w systemie.");

                NewOffer.SalespersonId = fallbackSalespersonId;
            }
        }

        decimal margin = 0;
        // buduj graf encji (prościej i bezpieczniej)
        NewOffer.CustomerId = customerId;
        foreach (var configuration in NewVehicleConfigurations)
        {
            //calculate price
            margin += 100;
            _db.OfferVehicleConfigurations.Add(configuration);
        }

        NewOfferVersion.Margin = margin;

        NewOffer.Versions.Add(NewOfferVersion);
        NewOffer.PdfDocument = NewPdfDocument;
        if (!ModelState.IsValid)
            return Page();
        _db.Offers.Add(NewOffer);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Salesperson/CustomerOffers", new { customerId });
    }

    public async Task<IActionResult> OnPostCalculateMargin(string payload, decimal discount)
    {
        await LoadVehicleComponentsAsync();
        var configs = JsonSerializer.Deserialize<List<VehicleConfigDto>>(
            payload,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? new List<VehicleConfigDto>();
        
        var cab = VehicleCabSizes.ToDictionary(x => x.Id, x => x.Price);
        var eng = VehicleEngines.ToDictionary(x => x.Id, x => x.Price);
        var gb  = VehicleGearboxes.ToDictionary(x => x.Id, x => x.Price);
        var it  = VehicleInteriors.ToDictionary(x => x.Id, x => x.Price);
        var dt  = VehicleDrivetrains.ToDictionary(x => x.Id, x => x.Price);

        var vehicles = new List<object>();
        decimal totalPrice = 0m;
        decimal totalMargin = 0m;

        foreach (var c in configs)
        {
            decimal price = 0m;

            if (c.size is int sizeId && cab.TryGetValue(sizeId, out var p1)) price += p1;
            if (c.engine is int engId && eng.TryGetValue(engId, out var p2)) price += p2;
            if (c.gearbox is int gbId && gb.TryGetValue(gbId, out var p3)) price += p3;
            if (c.interior is int itId && it.TryGetValue(itId, out var p4)) price += p4;
            if (c.drivetrain is int dtId && dt.TryGetValue(dtId, out var p5)) price += p5;

            totalPrice += price;

            // tymczasowo: per-vehicle finalPrice = price (rabat rozliczasz na totalu)
            vehicles.Add(new
            {
                price,
                finalPrice = price,
                margin = 0m
            });
        }

        // rabat na total
        var totalFinalPrice = totalPrice - discount;
        if (totalFinalPrice < 0) totalFinalPrice = 0; // opcjonalnie

        return new JsonResult(new
        {
            vehicles,
            total = new
            {
                price = totalPrice,
                finalPrice = totalFinalPrice,
                margin = totalMargin
            }
        });
    }
}

public class VehicleConfigDto
{
    public int? size { get; set; }
    public int? engine { get; set; }
    public int? gearbox { get; set; }
    public int? interior { get; set; }
    public int? drivetrain { get; set; }
}
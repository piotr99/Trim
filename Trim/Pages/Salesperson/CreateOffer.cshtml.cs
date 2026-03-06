using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Trim.Helpers;
using Trim.Models.BussinesCalculactions;

namespace Trim.Pages.Salesperson.Partial;

public class CreateOffer : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOfferCalculator _calc;

    public CreateOffer(ApplicationDbContext db, UserManager<ApplicationUser> userManager,  IOfferCalculator calc)
    {
        _db = db;
        _userManager = userManager;
        _calc = calc;
    }

    [BindProperty(SupportsGet = true)]
    public int customerId { get; set; }

    [BindProperty]
    public Offer NewOffer { get; set; } = new();

    [BindProperty]
    public List<OfferVehicleConfiguration> NewVehicleConfigurations { get; set; } = new();
    public OfferStatusEnum offerStatus { get; set; } = OfferStatusEnum.DRAFT; 
    // Listy pomocnicze do UI
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
    private async Task<int> DetermineSalespersonId(ApplicationUser? user)
    {
        // 1. Jeśli aktualnie zalogowany użytkownik jest handlowcem, przypisz jego ID
        if (user != null && await _userManager.IsInRoleAsync(user, "Salesperson"))
        {
            return user.Id;
        }

        // 2. Jeśli nie, spróbuj pobrać handlowca przypisanego bezpośrednio do klienta
        var customer = await _db.Customers
            .AsNoTracking()
            .Select(c => new { c.Id, c.SalespersonId }) // Optymalizacja: pobieramy tylko potrzebne pola
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer?.SalespersonId != null)
        {
            return customer.SalespersonId.Value;
        }

        // 3. Fallback: weź pierwszego handlowca z bazy (np. domyślny opiekun)
        var fallbackSalespersonId = await _db.Users
            .Where(u => EF.Property<string>(u, "UserType") == "Salesperson")
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        return fallbackSalespersonId; 
        // Zwróci 0, jeśli w systemie nie ma absolutnie żadnego handlowca.
    }

    public async Task OnGetAsync()
    {
        await LoadVehicleComponentsAsync();
        
        if (NewVehicleConfigurations.Count == 0)
            NewVehicleConfigurations.Add(new OfferVehicleConfiguration());

        NewOffer = new Offer
        {
            // Poprawiony format daty: yyyyMMdd (MM to miesiące, mm to minuty!)
            OfferFriendlyName = $"OFF-{DateTime.UtcNow:yyyyMMdd}-"
        };
    }

    public async Task<IActionResult> OnPostCreateOfferAsync()
    {
        await LoadVehicleComponentsAsync();
        var user = await _userManager.GetUserAsync(User);

        // 1. Logika przypisania handlowca (skrócona dla czytelności)
        NewOffer.SalespersonId = await DetermineSalespersonId(user);
        NewOffer.CustomerId = customerId;
        NewOffer.Status = offerStatus;

        // 2. PRZYGOTOWANIE DANYCH DO OBLICZEŃ
        // Mapujemy konfiguracje z formularza na DTO, które akceptuje metoda licząca
        var configsForCalc = NewVehicleConfigurations.Select(c => new VehicleConfigDto
        {
            size = c.SizeId,
            engine = c.EngineId,
            gearbox = c.GearboxId,
            interior = c.InteriorId,
            drivetrain = c.DrivetrainId,          // <-- popraw nazwę (u Ciebie było drivetrainId)
            additionalPrice = c.AdditionalPrice   // <-- dopłata
        }).ToList();

        // 3. WYWOŁANIE WSPÓLNEJ METODY LICZĄCEJ
        var calculations = await _calc.GetCalculationsAsync(configsForCalc, NewOffer.Discount);

        // 4. PRZYPISANIE WYNIKÓW DO OFERTY I JEJ ELEMENTÓW
        NewOffer.Price = calculations.Price;
        NewOffer.FinalPrice = calculations.FinalPrice;
        NewOffer.Bonus = calculations.Bonus;

        for (int i = 0; i < NewVehicleConfigurations.Count; i++)
        {
            NewVehicleConfigurations[i].Price = calculations.Vehicles[i].Price;
            NewVehicleConfigurations[i].Bonus = calculations.Vehicles[i].Bonus;
            NewVehicleConfigurations[i].BonusMultiplier = calculations.Vehicles[i].BonusMultiplier; // jeśli masz w encji

            NewOffer.OfferVehicleConfigurations.Add(NewVehicleConfigurations[i]);
        }

        if (!ModelState.IsValid)
        {
            await LoadVehicleComponentsAsync();
            return Page();
        }

        _db.Offers.Add(NewOffer);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Salesperson/CustomerOffers", new { customerId });
    }

    // HANDLER DLA AJAX
    public async Task<IActionResult> OnPostCalculateMargin(string payload, decimal discount)
    {
        await LoadVehicleComponentsAsync();

        var configs = JsonSerializer.Deserialize<List<VehicleConfigDto>>(
            payload,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? new List<VehicleConfigDto>();

        var calcResult = _calc.GetCalculationsAsync(configs, discount);
        
        return new JsonResult(new
        {
            vehicles = calcResult.Result.Vehicles.Select(v => new {
                price = v.Price,
                bonus = v.Bonus,
                bonusMultiplier = v.BonusMultiplier
            }),
            total = new
            {
                price = calcResult.Result.Price,
                finalPrice = calcResult.Result.FinalPrice,
                bonus = calcResult.Result.Bonus
            }
        });
    }
}
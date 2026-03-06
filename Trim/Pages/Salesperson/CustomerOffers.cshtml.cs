using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Helpers;
using Trim.Models;
using Trim.Models.ViewModels;
using Trim.Models.BussinesCalculactions;

namespace Trim.Pages.Salesperson;

public class CustomerOffers : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IOfferCalculator _calc;

    public CustomerOffers(ApplicationDbContext db,  IOfferCalculator calc)
    {
        _db = db;
        _calc = calc;
    }

    public List<Offer> Offers { get; set; } = new();
    public List<OffersVm> OffersVm { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int customerId { get; set; }
    public Customer? Customer { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }
    public Offer? SelectedOffer { get; set; } 
    public Offer? NewOffer {get; set;}
    public async Task<PartialViewResult> OnPostOffersListPartialAsync(int customerId, string? filter)
    {
        var q = _db.Offers
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = $"%{filter.Trim()}%";
            q = q.Where(o => EF.Functions.Like(o.OfferFriendlyName, f));
        }

        OffersVm = await q
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OffersVm
            {
                OfferId = o.Id,
                OfferFriendlyName = o.OfferFriendlyName,
                Status = o.Status
            })
            .ToListAsync();

        return Partial("Salesperson/Partial/_OffersList", OffersVm);
    }
    public async Task<IActionResult> OnGetAsync()
    {
        if (customerId <= 0)
            return RedirectToPage("/Salesperson/CustomerOffers");

        Customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (Customer == null)
            return RedirectToPage("/Salesperson/CustomerOffers");


        OnPostOffersListPartialAsync(customerId, "");
        return Page();
    }

    public async Task<PartialViewResult> OnPostOfferDetailsPartialAsync([FromForm] int offerId)
    {
        var offer = await _db.Offers
            .AsNoTracking()
            .Include(o => o.OfferVehicleConfigurations)
            .ThenInclude(vc => vc.Size)
            .Include(o => o.OfferVehicleConfigurations)
            .ThenInclude(vc => vc.Engine)
            .Include(o => o.OfferVehicleConfigurations)
            .ThenInclude(vc => vc.Gearbox)
            .Include(o => o.OfferVehicleConfigurations)
            .ThenInclude(vc => vc.Interior)
            .Include(o => o.OfferVehicleConfigurations)
            .ThenInclude(vc => vc.Drivetrain)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer is null)
        {
            // możesz też zwrócić pusty partial z komunikatem
            return Partial("Salesperson/Partial/_OfferDetails", new OfferDetailsVm
            {
                Offer = null,
                OfferVehicleConfigurations = new List<OfferVehicleConfiguration>()
            });
        }

        var vm = new OfferDetailsVm
        {
            Offer = offer,
            OfferVehicleConfigurations = offer.OfferVehicleConfigurations?.ToList()
                                         ?? new List<OfferVehicleConfiguration>()
        };

        return Partial("Salesperson/Partial/_OfferDetails", vm);
    }

    public async Task<PartialViewResult> OnPostEditOfferPartialAsync([FromForm] int offerId)
    {
        var currentOffer = await _db.Offers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == offerId);
        if (currentOffer is null)
        {
            // możesz zwrócić partial z błędem
            return Partial("Salesperson/Partial/_Error", "Nie znaleziono oferty.");
        }
        var vm = new EditOfferVm
        {
            OfferId =  offerId,
            CustomerId = currentOffer.CustomerId,
            OfferFriendlyName = currentOffer.OfferFriendlyName,
            Status = currentOffer.Status,
            FinalPrice = currentOffer.FinalPrice,
            Price = currentOffer.Price,
            Bonus = currentOffer.Bonus,
            Discount = currentOffer.Discount,
            Vehicles = await _db.OfferVehicleConfigurations.Where(v => v.OfferId == offerId).ToListAsync(),
            VehicleCabSizes = await  _db.VehicleCabSizes.ToListAsync(),
            VehicleDrivetrains = await _db.VehicleDrivetrains.ToListAsync(),
            VehicleEngines = await _db.VehicleEngines.ToListAsync(),
            VehicleGearboxes = await _db.VehicleGearboxes.ToListAsync(),
            VehicleInteriors = await _db.VehicleInteriors.ToListAsync(),
        };
        return Partial("Salesperson/Partial/_EditOffer", vm);

    }

    public async Task<JsonResult> OnPostUpdatePricesPartialAsync([FromForm] OfferDetailsVm vm)
    {
        if (vm?.Offer == null)
            return new JsonResult(new { error = "Brak danych oferty." });
        
        
        
        var configs = vm.OfferVehicleConfigurations ?? new List<OfferVehicleConfiguration>();

        var configsForCalc = configs.Select(c => new VehicleConfigDto
        {
            size = c.SizeId,
            engine = c.EngineId,
            gearbox = c.GearboxId,
            interior = c.InteriorId,
            drivetrain = c.DrivetrainId,
            additionalPrice = c.AdditionalPrice
        }).ToList();

        var calc = await _calc.GetCalculationsAsync(configsForCalc, vm.Offer.Discount);

        // Zwracamy dane do UI (bez zapisu do DB)
        return new JsonResult(new
        {
            vehicles = calc.Vehicles.Select(v => new
            {
                price = v.Price,
                bonus = v.Bonus,
                bonusMultiplier = v.BonusMultiplier
            }),
            total = new
            {
                price = calc.Price,
                finalPrice = calc.FinalPrice,
                bonus = calc.Bonus
            }
        });
    }
    public async Task<PartialViewResult> OnPostUpdateOfferPartialAsync([FromForm] EditOfferVm vm)
{
    // 1) walidacja podstawowa
    if (vm?.OfferId == null)
    {
        ModelState.AddModelError("", "Niepoprawne dane oferty.");
        return Partial("Salesperson/Partial/_OfferDetails", vm);
    }

    // 2) pobierz istniejącą ofertę + konfiguracje (tracking ON)
    var offer = await _db.Offers
        .Include(o => o.OfferVehicleConfigurations)
        .FirstOrDefaultAsync(o => o.Id == vm.OfferId);

    if (offer is null)
    {
        ModelState.AddModelError("", "Nie znaleziono oferty.");
        return Partial("Salesperson/Partial/_OfferDetails", vm);
    }

    // 3) aktualizuj pola edytowalne (bez Price/Bonus/FinalPrice)
    offer.OfferFriendlyName = vm.OfferFriendlyName;
    offer.Discount = vm.Discount;
    offer.Status = vm.Status;

    // 4) DIFF konfiguracji pojazdów: update / insert / delete
    var incoming = vm.Vehicles ?? new List<OfferVehicleConfiguration>();
    var incomingById = incoming.Where(x => x.Id > 0).ToDictionary(x => x.Id);

    // usuń te, które są w DB, ale nie przyszły z UI
    var toRemove = offer.OfferVehicleConfigurations
        .Where(dbv => dbv.Id > 0 && !incomingById.ContainsKey(dbv.Id))
        .ToList();

    if (toRemove.Count > 0)
    {
        _db.OfferVehicleConfigurations.RemoveRange(toRemove);
        foreach (var r in toRemove) offer.OfferVehicleConfigurations.Remove(r); // spójność kolekcji
    }

    // update istniejących
    foreach (var dbv in offer.OfferVehicleConfigurations.Where(x => x.Id > 0))
    {
        if (!incomingById.TryGetValue(dbv.Id, out var inp)) continue;

        dbv.SizeId = inp.SizeId;
        dbv.EngineId = inp.EngineId;
        dbv.GearboxId = inp.GearboxId;
        dbv.InteriorId = inp.InteriorId;
        dbv.DrivetrainId = inp.DrivetrainId;
        dbv.AdditionalPrice = inp.AdditionalPrice;
        dbv.AdditionalEquipment = inp.AdditionalEquipment;
    }

    // dodaj nowe (Id==0)
    foreach (var inp in incoming.Where(x => x.Id == 0))
    {
        offer.OfferVehicleConfigurations.Add(new OfferVehicleConfiguration
        {
            SizeId = inp.SizeId,
            EngineId = inp.EngineId,
            GearboxId = inp.GearboxId,
            InteriorId = inp.InteriorId,
            DrivetrainId = inp.DrivetrainId,
            AdditionalPrice = inp.AdditionalPrice,
            AdditionalEquipment = inp.AdditionalEquipment
        });
    }

    // 5) przelicz ceny/bonus (backend jest źródłem prawdy)
    var configsForCalc = offer.OfferVehicleConfigurations.Select(c => new VehicleConfigDto
    {
        size = c.SizeId,
        engine = c.EngineId,
        gearbox = c.GearboxId,
        interior = c.InteriorId,
        drivetrain = c.DrivetrainId,
        additionalPrice = c.AdditionalPrice
    }).ToList();

    var calc = await _calc.GetCalculationsAsync(configsForCalc, offer.Discount);

    offer.Price = calc.Price;
    offer.FinalPrice = calc.FinalPrice;
    offer.Bonus = calc.Bonus;

    var i = 0;
    foreach (var cfg in offer.OfferVehicleConfigurations)
    {
        cfg.Price = calc.Vehicles[i].Price;
        cfg.Bonus = calc.Vehicles[i].Bonus;
        cfg.BonusMultiplier = calc.Vehicles[i].BonusMultiplier;
        i++;
    }

    await _db.SaveChangesAsync();

    // 6) zwróć świeże dane z nawigacjami do widoku (AsNoTracking)
    var fresh = await _db.Offers.AsNoTracking()
        .Include(o => o.OfferVehicleConfigurations).ThenInclude(x => x.Size)
        .Include(o => o.OfferVehicleConfigurations).ThenInclude(x => x.Engine)
        .Include(o => o.OfferVehicleConfigurations).ThenInclude(x => x.Gearbox)
        .Include(o => o.OfferVehicleConfigurations).ThenInclude(x => x.Interior)
        .Include(o => o.OfferVehicleConfigurations).ThenInclude(x => x.Drivetrain)
        .FirstAsync(o => o.Id == offer.Id);

    var outVm = new OfferDetailsVm
    {
        Offer = fresh,
        OfferVehicleConfigurations = fresh.OfferVehicleConfigurations.ToList()
    };

    return Partial("Salesperson/Partial/_OfferDetails", outVm);
}


}
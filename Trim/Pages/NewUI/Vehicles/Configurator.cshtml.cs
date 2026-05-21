using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;
using Trim.Helpers;

namespace Trim.Pages.NewUI.Vehicles
{
    public class ConfiguratorModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ICallFactoryForVinHelper _vinHelper;
        private readonly IUpdateOfferPricing _pricingHelper;

        public ConfiguratorModel(ApplicationDbContext db, ICallFactoryForVinHelper vinHelper, IUpdateOfferPricing pricingHelper)
        {
            _db = db;
            _vinHelper = vinHelper;
            _pricingHelper = pricingHelper;
  
        }

        // To łapie ID oferty z adresu URL (np. /Configurator?id=5)
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public void OnGet()
        {
            // Zwykłe załadowanie pustej strony HTML (JS zrobi resztę)
        }

        // HANDLER 1: Wysyła słowniki do JavaScriptu przy ładowaniu strony
        public async Task<IActionResult> OnGetGetConfigurationAsync()
        {
            var data = new
            {
                cabSizes = await _db.VehicleCabSizes.Select(x => new { id = x.Id, name = x.Name, price = x.Price }).ToListAsync(),
                engines = await _db.VehicleEngines.Select(x => new { id = x.Id, name = x.Name, price = x.Price }).ToListAsync(),
                gearboxes = await _db.VehicleGearboxes.Select(x => new { id = x.Id, name = x.Name, price = x.Price }).ToListAsync(),
                interiors = await _db.VehicleInteriors.Select(x => new { id = x.Id, name = x.Name, price = x.Price }).ToListAsync(),
                drivetrains = await _db.VehicleDrivetrains.Select(x => new { id = x.Id, name = x.Name, price = x.Price }).ToListAsync(),
            };

            return new JsonResult(data);
        }

        // HANDLER 2: Odbiera i zapisuje skonfigurowany pojazd
        public async Task<IActionResult> OnPostSaveConfigurationAsync(ConfigurationInputDto input)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { error = "Przesłano niekompletne dane." }) { StatusCode = 400 };
            }
            if (input.OfferId == 0 || input.OfferId == null)
            {
                return new JsonResult(new { error = "Brak ID" }) { StatusCode = 400 };
            }

            // 1. POBIERAMY ELEMENTY Z BAZY (Weryfikacja istnienia i ceny)
            var cab = await _db.VehicleCabSizes.FindAsync(input.SizeId);
            var engine = await _db.VehicleEngines.FindAsync(input.EngineId);
            var gearbox = await _db.VehicleGearboxes.FindAsync(input.GearboxId);
            var interior = await _db.VehicleInteriors.FindAsync(input.InteriorId);
            var drivetrain = await _db.VehicleDrivetrains.FindAsync(input.DrivetrainId);

            if (cab == null || engine == null || gearbox == null || interior == null || drivetrain == null)
            {
                return new JsonResult(new { error = "Wybrano nieprawidłowe opcje konfiguracyjne." }) { StatusCode = 400 };
            }

            decimal calculatedPrice = cab.Price + engine.Price + gearbox.Price + interior.Price + drivetrain.Price;

            var config = new VehicleConfiguration
            {
                SizeId = cab.Id,
                EngineId = engine.Id,
                GearboxId = gearbox.Id,
                InteriorId = interior.Id,
                DrivetrainId = drivetrain.Id,
                Price = calculatedPrice,
                AdditionalPrice = input.AdditionalPrice ?? 0,
                AdditionalEquipment = input.AdditionalEquipment,
                Bonus = 0,
                BonusMultiplier = 0
            };

            string vin = await _vinHelper.GetVinAsync(config);

            // 4. TWORZENIE POJAZDU
            var vehicle = new Vehicle
            {
                Name = input.VehicleName,
                Vin = vin,
                Status = VehicleStatusEnum.AVAILABLE, // Domyślny status po stworzeniu
                Configuration = config,

                // Kluczowe: Przypinamy auto do oferty, z której weszliśmy w konfigurator
                OfferId = input.OfferId
            };

            // 5. ZAPIS DO BAZY
            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();

            //Oblcizanie cen na ofercie:
            var vehicles = await _db.Vehicles
                .Include(v => v.Configuration)
                .Where(v => v.OfferId == input.OfferId)
                .ToListAsync();

            var pricing = await _pricingHelper.Update(vehicles);
            var offerToUpdate = await _db.Offers.FindAsync(input.OfferId);

            if (offerToUpdate != null)
            {
                offerToUpdate.Price = pricing[0];
                offerToUpdate.Discount = pricing[1];
                offerToUpdate.Bonus = pricing[2];
                offerToUpdate.FinalPrice = pricing[3];

                await _db.SaveChangesAsync();
            }

            return new JsonResult(new { success = true, vehicleId = vehicle.Id });
        }
    }
    public class ConfigurationInputDto
    {
        public int OfferId { get; set; }
        public string VehicleName { get; set; } = default!;
        public int SizeId { get; set; }
        public int EngineId { get; set; }
        public int GearboxId { get; set; }
        public int InteriorId { get; set; }
        public int DrivetrainId { get; set; }
        public string? AdditionalEquipment { get; set; }
        public decimal? AdditionalPrice { get; set; }
    }
}
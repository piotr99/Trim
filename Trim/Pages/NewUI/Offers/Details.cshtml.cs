using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Helpers;
using Trim.Models;

namespace Trim.Pages.NewUI.Offers
{
    public class DetailsModel : PageModel
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUpdateOfferPricing _pricingHelper;


        public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IUpdateOfferPricing pricingHelper)
        {
            _db = db;
            _userManager = userManager;
            _pricingHelper = pricingHelper;
        }

        // Właściwość, przez którą przekażemy dane do widoku HTML
        public SalesCase CurrentCase { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetLoadOfferAsync()
        {
            // 1. Zabezpieczenie asynchroniczne i projekcja - BEZ ZBĘDNYCH INCLUDE!
            var offerData = await _db.SalesCases
                .AsNoTracking() // Zawsze dobre przy samym odczycie
                .Where(sc => sc.Id == Id)
                .Select(sc => new
                {
                    id = sc.Id,
                    offerFriendlyName = sc.Offer.OfferFriendlyName,
                    status = sc.Offer.Status.ToString(),

                    // Pobieramy czystą datę (formatowanie zrobimy w pamięci, żeby EF nie wyrzucił błędu SQL)
                    createdAt = sc.Offer.CreatedAt,

                    // Kasa
                    price = sc.Offer.Price,
                    discount = sc.Offer.Discount,
                    bonus = sc.Offer.Bonus,
                    finalPrice = sc.Offer.FinalPrice,

                    // EF Core sam wykona "LEFT JOIN Customers" tylko po to, by wziąć te 3 kolumny
                    customer = new
                    {
                        id = sc.CustomerId,
                        name = sc.Customer.CompanyName ?? (sc.Customer.FirstName + " " + sc.Customer.LastName)
                    },

                    // EF Core sam wykona "LEFT JOIN AspNetUsers" 
                    salesperson = sc.AssignedSalesperson != null
                        ? sc.AssignedSalesperson.FirstName + " " + sc.AssignedSalesperson.LastName
                        : "Brak",

                    // EF Core sam zbuduje JOINY do tablicy pojazdów i wszystkich słowników!
                    vehicles = sc.Offer.Vehicles.Select(v => new
                    {
                        id = v.Id,
                        cabSize = v.Configuration.Size.Name, // Zakładam, że pole ze słownika to Name
                        engine = v.Configuration.Engine.Name,
                        gearbox = v.Configuration.Gearbox.Name,
                        interior = v.Configuration.Interior.Name,
                        drivetrain = v.Configuration.Drivetrain.Name,

                        price = v.Configuration.Price,
                        additionalPrice = v.Configuration.AdditionalPrice,
                        additionalEquipment = v.Configuration.AdditionalEquipment
                    }).ToList()
                })
                .FirstOrDefaultAsync(); // Wykonujemy w pełni asynchronicznie

            // 2. Obsługa braku oferty
            if (offerData == null)
            {
                return new JsonResult(new { error = "Nie znaleziono oferty o podanym ID." }) { StatusCode = 404 };
            }

            // 3. (Opcjonalnie) Formatowanie daty, które bezpieczniej robić po wyciągnięciu danych z SQL
            var finalResult = new
            {
                offerData.id,
                offerData.offerFriendlyName,
                offerData.status,
                createdAt = offerData.createdAt?.ToString("dd.MM.yyyy HH:mm"), // Formatujemy
                offerData.price,
                offerData.discount,
                offerData.bonus,
                offerData.finalPrice,
                offerData.customer,
                offerData.salesperson,
                offerData.vehicles
            };

            // 4. Zwracamy leciutki, optymalny JSON
            return new JsonResult(finalResult);
        }
        public async Task<IActionResult> OnPostDeleteVehicleAsync(int vehicleId, int Id)
        {
            // 1. Wstępna walidacja
            if (vehicleId <= 0)
            {
                return new JsonResult(new { error = "Nieprawidłowe ID pojazdu." }) { StatusCode = 400 };
            }

            var statusCheck = await _db.SalesCases.AsNoTracking()
                .Where(sc => sc.Id == Id)
                .Select(sc => sc.Offer).FirstOrDefaultAsync();
            if (statusCheck.Status != OfferStatusEnum.DRAFT && statusCheck.Status != OfferStatusEnum.IN_NEGOTIATION)
            {
                return new JsonResult(new { error = "Poj" }) { StatusCode = 400 };
            }
            // 2. POBRANIE ID OFERTY ZANIM USUNIEMY POJAZD
            // Wyciągamy tylko OfferId, żeby było super szybkie (bez ładowania całego obiektu)
            var offerId = await _db.Vehicles
                .Where(v => v.Id == vehicleId)
                .Select(v => v.OfferId)
                .FirstOrDefaultAsync();

            if (offerId == null)
            {
                return new JsonResult(new { error = "Pojazd nie istnieje lub nie jest przypisany do żadnej oferty." }) { StatusCode = 404 };
            }

            bool isOnParkingLot = await _db.Vehicles
                .Where(v => v.Id == vehicleId)
                .Select(v => v.ParkingLot)
                .FirstOrDefaultAsync();

            if (isOnParkingLot)
            {
                var sentToParkingLot = await _db.Vehicles
                .Where(v => v.Id == vehicleId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.OfferId, (int?)null)
                    .SetProperty(o => o.CustomerId, (int?)null));
            }
            else
            {
                var deletedRows = await _db.Vehicles
                .Where(v => v.Id == vehicleId)
                .ExecuteDeleteAsync();

                if (deletedRows == 0)
                {
                    return new JsonResult(new { error = "Nie udało się usunąć pojazdu z bazy." }) { StatusCode = 500 };
                }
            }
            

            

            // 4. POBRANIE POZOSTAŁYCH POJAZDÓW DLA TEJ SAMEJ OFERTY
            var remainingVehicles = await _db.Vehicles
                .Include(v => v.Configuration) // Niezbędne dla Twojego helpera do wyliczenia cen
                .Where(v => v.OfferId == offerId)
                .ToListAsync();

            // 5. PRZELICZENIE CEN I AKTUALIZACJA OFERTY
            var pricing = await _pricingHelper.Update(remainingVehicles);

            var offerToUpdate = await _db.Offers.FindAsync(offerId);
            if (offerToUpdate != null)
            {
                // pricing[0] = Cena bazowa, pricing[1] = Rabat, pricing[2] = Bonus, pricing[3] = Cena Finalna
                offerToUpdate.Price = pricing[0];
                offerToUpdate.Discount = pricing[1];
                offerToUpdate.Bonus = pricing[2];
                offerToUpdate.FinalPrice = pricing[3];

                await _db.SaveChangesAsync();
            }

            // 6. Zwrócenie sukcesu (skrypt JS przeładuje stronę i pokaże zaktualizowane sumy)
            return new JsonResult(new { success = true });
        }
        public async Task<IActionResult> OnPostSendOfferAsync(int id)
        {
            if (id == 0 || id == null)
            {
                return new JsonResult(new { success = false });
            }
            //pobieram oferte z case i zmieniam status na SENT
            var currentOffer = await _db.SalesCases
                .Include(sc => sc.Offer)
                .Where(sc => sc.Id == id)
                .FirstOrDefaultAsync();
            currentOffer.Offer.Status = OfferStatusEnum.SENT;

            await _db.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
        public async Task<IActionResult> OnPostCancelOfferAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new { success = false, error = "Nieprawidłowe ID oferty." }) { StatusCode = 400 };
            }

            var offer = await _db.Offers
                .FirstOrDefaultAsync(o => o.Id == id);

            if (offer == null)
            {
                return new JsonResult(new { success = false, error = "Nie znaleziono oferty." }) { StatusCode = 404 };
            }

            offer.Status = OfferStatusEnum.DRAFT;

            await _db.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}

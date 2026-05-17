using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.NewUI.Offers
{
    public class DetailsModel : PageModel
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;


        public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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
                    status = sc.Status.ToString(),

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
                        id = v.Configuration.Id,
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
    }
}

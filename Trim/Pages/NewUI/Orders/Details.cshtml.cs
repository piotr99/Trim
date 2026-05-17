using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.NewUI.Orders
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

        public SalesCase CurrentCase { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetLoadOffersAsync()
        {
            var orderData = await _db.SalesCases
                .AsNoTracking()
                .Where(sc => sc.Id == Id)
                .Select(sc => new {
                    id = sc.Id,
                    createdAt = sc.Order.CreatedAt,
                    orderNumber = sc.Order.OrderNumber,
                    status = sc.Order.Status.ToString(),
                    finalPrice = sc.Order.FinalPrice,
                    customer = new
                    {
                        id = sc.CustomerId,
                        name = sc.Customer.CompanyName ?? (sc.Customer.FirstName + " " + sc.Customer.LastName)
                    },
                    salesperson = sc.AssignedSalesperson != null
                        ? sc.AssignedSalesperson.FirstName + " " + sc.AssignedSalesperson.LastName
                        : "Brak",
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
                }).FirstOrDefaultAsync();

            if (orderData == null)
            {
                return new JsonResult(new { error = "Nie znaleziono oferty o podanym ID." }) { StatusCode = 404 };
            }

            return new JsonResult(orderData);
        } 
    }
}

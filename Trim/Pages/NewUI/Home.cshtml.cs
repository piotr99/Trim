using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Trim.Pages.NewUI
{
    public class HomeModel : PageModel
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        public HomeModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        

        public async Task<IActionResult> OnGetGetOffersAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return new JsonResult(new { error = "Użytkownik niezalogowany" }) { StatusCode = 401 };
            }

            var offersData = await _db.SalesCases
                .AsNoTracking()
                .Where(sc => sc.AssignedSalespersonId == currentUser.Id)
                .Where(sc => sc.Order == null)
                .Where(sc => sc.Offer != null)
                .Select(sc => new
                {
                    id = sc.Id,
                    offerFriendlyName = sc.Offer.OfferFriendlyName,
                    customerName = sc.Customer.CompanyName ?? (sc.Customer.FirstName + " " + sc.Customer.LastName),
                    status = sc.Status.ToString(),
                    vehicles = sc.Offer.Vehicles.Count
                }).ToListAsync();

            // 5. Zwracamy jako JSON
            return new JsonResult(offersData);
        }
        public async Task<IActionResult> OnGetGetOrdersAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return new JsonResult(new { error = "Użytkownik niezalogowany" }) { StatusCode = 401 };
            }

            var ordersData = await _db.SalesCases
                .AsNoTracking()
                .Where(sc => sc.AssignedSalespersonId == currentUser.Id)
                .Where(sc => sc.Order != null)
                .Select(sc => new
                {
                    id = sc.Id,
                    orderNumber = sc.Order.OrderNumber,
                    customerName = sc.Customer.CompanyName ?? (sc.Customer.FirstName + " " + sc.Customer.LastName),
                    status = sc.Status.ToString()
                }).ToListAsync();

            return new JsonResult(ordersData);
        }
        }
}

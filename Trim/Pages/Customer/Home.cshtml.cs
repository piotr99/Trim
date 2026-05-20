using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.User
{
    public class HomeModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUser CurrentUser { get; set; }
        public List<SalesCase> SaleCases { get; set; }
        public int RequiresAttention = 0;
        public int CustomerVehicles = 0;
        public HomeModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Pobranie użytkownika
            CurrentUser = await _userManager.GetUserAsync(User);

            // 1. Zabezpieczenie: jeśli użytkownik nie istnieje, odeślij do logowania
            if (CurrentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // 2. Poprawiony warunek: sprawdzamy czy wartość NIE jest true (znak wykrzyknika)
            if (!await _userManager.IsInRoleAsync(CurrentUser, "Customer"))
            {
                // Najlepiej używać RedirectToPage dla stron Razor Pages
                return RedirectToPage("/Account/Logout");
            }

            SaleCases = await _db.SalesCases
                .AsNoTracking()
                .Where(sc => sc.CustomerId == CurrentUser.Id)
                .Include(sc => sc.AssignedSalesperson)
                .Include(sc => sc.ActivityLogs)
                .Include(sc => sc.Offer)
                .ToListAsync();

            RequiresAttention = SaleCases
                .Where(sc => sc.Offer != null && sc.Offer.Status == OfferStatusEnum.SENT)
                .Select(sc => sc.Offer)
                .Count();

            CustomerVehicles = await _db.Users.OfType<Trim.Models.Customer>()
                .Where(c => c.Id == CurrentUser.Id)
                .SelectMany(c => c.Vehicles)
                .CountAsync();
            // 3. Poprawiony return: wyrenderuj stronę (Home.cshtml)
            return Page();
        }
    }
}
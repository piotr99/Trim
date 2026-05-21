using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.Customer
{
    public class OrderModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public SalesCase CurrentCase { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; } // Oczekujemy ID Zamówienia (OrderId)

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUserIdStr = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserIdStr))
            {
                return RedirectToPage("/Account/Login");
            }

            // Dociągamy sprawę, powiązane ZAMÓWIENIE i wszystkie słowniki konfiguracji
            CurrentCase = await _db.SalesCases
                .AsNoTracking()
                .Include(sc => sc.Order)
                    .ThenInclude(o => o.Vehicles)
                        .ThenInclude(v => v.Configuration)
                            .ThenInclude(c => c.Engine)
                .Include(sc => sc.Order)
                    .ThenInclude(o => o.Vehicles)
                        .ThenInclude(v => v.Configuration)
                            .ThenInclude(c => c.Gearbox)
                .Include(sc => sc.Order)
                    .ThenInclude(o => o.Vehicles)
                        .ThenInclude(v => v.Configuration)
                            .ThenInclude(c => c.Size)
                .Include(sc => sc.Order)
                    .ThenInclude(o => o.Vehicles)
                        .ThenInclude(v => v.Configuration)
                            .ThenInclude(c => c.Interior)
                .Include(sc => sc.Order)
                    .ThenInclude(o => o.Vehicles)
                        .ThenInclude(v => v.Configuration)
                            .ThenInclude(c => c.Drivetrain)
                .FirstOrDefaultAsync(sc => sc.Order != null && sc.Id == Id);

            if (CurrentCase == null || CurrentCase.Order == null)
            {
                return RedirectToPage("/Customer/Home");
            }

            // Zabezpieczenie: czy zamówienie należy do aktualnie zalogowanego klienta
            if (CurrentCase.CustomerId.ToString() != currentUserIdStr)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            return Page();
        }
    }
}
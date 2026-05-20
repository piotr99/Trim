using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Trim.DbContext;
using Trim.Models;
using Trim.Helpers;

namespace Trim.Pages.Customer
{
    public class OfferModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderHelper _offerHelper;

        public OfferModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IOrderHelper offerHelper)
        {
            _db = db;
            _userManager = userManager;
            _offerHelper = offerHelper;
        }

        public SalesCase CurrentCase { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGet()
        {
            // 1. Pobranie identyfikatora zalogowanego klienta (dużo szybsze niż ładowanie całego obiektu usera)
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentCase = await _db.SalesCases
            .AsNoTracking()
            .Include(sc => sc.Offer)
                .ThenInclude(o => o.Vehicles)
                    .ThenInclude(v => v.Configuration)
                        .ThenInclude(c => c.Engine)
            .Include(sc => sc.Offer)
                .ThenInclude(o => o.Vehicles)
                    .ThenInclude(v => v.Configuration)
                        .ThenInclude(c => c.Gearbox) 
            .Include(sc => sc.Offer)
                .ThenInclude(o => o.Vehicles)
                    .ThenInclude(v => v.Configuration)
                        .ThenInclude(c => c.Size)
            .Include(sc => sc.Offer)
                .ThenInclude(o => o.Vehicles)
                    .ThenInclude(v => v.Configuration)
                        .ThenInclude(c => c.Interior)
            .Include(sc => sc.Offer)
                .ThenInclude(o => o.Vehicles)
                    .ThenInclude(v => v.Configuration)
                        .ThenInclude(c => c.Drivetrain)
            .FirstOrDefaultAsync(sc => sc.Offer.Id == Id);

            // 3. Weryfikacja czy zgłoszenie i przypisana do niego oferta w ogóle istnieją
            if (CurrentCase == null || CurrentCase.Offer == null)
            {
                return RedirectToPage("/Customer/Home");
            }

            if (CurrentCase.CustomerId.ToString() != currentUserId)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            return Page();
        }
        public async Task<IActionResult> OnPostAcceptOfferAsync(int id)
        {
            var currentUserIdStr = _userManager.GetUserId(User);

            // Weryfikacja czy użytkownik jest zalogowany
            if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out int currentUserId))
            {
                return new JsonResult(new { success = false }) { StatusCode = 401 }; // Unauthorized
            }

            // Wyciągamy IDENTYFIKATOR KLIENTA, a nie identyfikator zgłoszenia!
            int customerId = await _db.SalesCases
                .Where(sc => sc.Id == id)
                .Select(sc => sc.CustomerId)
                .FirstOrDefaultAsync();

            // Weryfikacja czy zgłoszenie należy do tego klienta
            if (customerId != currentUserId)
            {
                return new JsonResult(new { success = false }) { StatusCode = 403 };
            }

            // Aktualizacja w bazie (używamy małego 'id' z parametru)
            await _db.Set<Offer>()
                .Where(c => c.SalesCaseId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, OfferStatusEnum.ACCEPTED));

            bool response = await _offerHelper.CreateOffer(id);
            if (!response)
            {
                return new JsonResult(new { success = false }) { StatusCode = 401 };
            }


            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostDeclineOfferAsync(int id)
        {
            var currentUserIdStr = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out int currentUserId))
            {
                return new JsonResult(new { success = false }) { StatusCode = 401 };
            }

            int customerId = await _db.SalesCases
                .Where(sc => sc.Id == id)
                .Select(sc => sc.CustomerId)
                .FirstOrDefaultAsync();

            if (customerId != currentUserId)
            {
                return new JsonResult(new { success = false }) { StatusCode = 403 };
            }

            await _db.Set<Offer>()
                .Where(c => c.SalesCaseId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, OfferStatusEnum.IN_NEGOTIATION));

            return new JsonResult(new { success = true });
        }
    }
}
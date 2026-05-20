using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.Customer
{
    public class CaseModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CaseModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public SalesCase SalesCase { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            SalesCase = await _db.SalesCases
                .AsNoTracking()
                .Include(sc => sc.AssignedSalesperson)
                .Include(sc => sc.ActivityLogs)
                .Include(sc => sc.Offer)
                .FirstOrDefaultAsync(sc => sc.Id == Id);

            if (SalesCase == null)
            {
                return RedirectToPage("/Customer/Home");
            }
            if (SalesCase.CustomerId.ToString() != currentUserId)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            // To potężna, bezpośrednia komenda SQL (przykład na przyszłość):
            await _db.Set<CustomerCommunication>()
                .Where(c => c.SalesCaseId == Id && c.ReadByCustomerAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ReadByCustomerAt, DateTime.UtcNow));

            return Page();
        }
        //ajax
        public async Task<IActionResult> OnPostSendMessageAsync(int id, string content)
        {
            var currentUserIdStr = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out int currentUserId))
            {
                return new JsonResult(new { success = false }) { StatusCode = 401 };
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return new JsonResult(new { success = false, message = "Wiadomość jest pusta." }) { StatusCode = 400 };
            }

            bool hasAccess = await _db.SalesCases.AnyAsync(sc => sc.Id == id && sc.CustomerId == currentUserId);

            if (!hasAccess)
            {
                return new JsonResult(new { success = false, error = "Brak dostępu" }) { StatusCode = 403 };
            }

            var newCommunication = new CustomerCommunication
            {
                SalesCaseId = id,
                MessageContent = content,
                IsPrivateMessage = false,
                SenderId = currentUserId,
                Direction = MessageDirectionEnum.INBOUND,
                ReadByCustomerAt = DateTime.UtcNow 
            };

            _db.Add(newCommunication);
            await _db.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
        public async Task<IActionResult> OnGetLoadCommentsAsync()
        {
            // 1. Pobieramy zgłoszenie wraz z logami na nowo dla tego konkretnego żądania AJAX
            var SalesCase = await _db.SalesCases
                .Include(sc => sc.ActivityLogs)
                .FirstOrDefaultAsync(sc => sc.Id == Id);

            if (SalesCase == null)
            {
                return new JsonResult(new { success = false, message = "Nie znaleziono zgłoszenia." });
            }

            // 2. Filtrujemy obecny, załadowany już zbiór (Twój poprawiony kod, bez await)
            var messages = SalesCase.ActivityLogs
                .Where(c => c.IsPrivateMessage == false)
                .OrderBy(c => c.SentAt)
                .Select(c => new
                {
                    c.Id,
                    c.MessageContent,
                    c.Direction,
                    c.SentAt
                })
                .ToList();

            // 3. Zwracamy czysty JSON
            return new JsonResult(new { success = true, activityLogs = messages });
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.IdentityModel.Tokens.Jwt;
using Trim.DbContext;
using Trim.Helpers;
using Trim.Models;
// Pamiętaj o dodaniu namespace'ów dla swoich modeli i bazy danych
namespace Trim.Pages.NewUI;
public class CaseModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISalespeopleHelper _salespeopleHelper;


    public CaseModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ISalespeopleHelper salespeopleHelper)
    {
        _db = db;
        _userManager = userManager;
        _salespeopleHelper = salespeopleHelper;
    }

    // Właściwość, przez którą przekażemy dane do widoku HTML
    public SalesCase CurrentCase { get; set; }
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    // Dodajemy parametr "int? id", który automatycznie złapie wartość z adresu URL
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            // Przekierowanie, jeśli brakuje parametru
            return RedirectToPage("/NewUI/home");
        }

        // Pobieramy sprawę na podstawie odebranego ID
        CurrentCase = await _db.SalesCases
            .Include(c => c.Customer)
            .Include(c => c.Offer)
            .Include(c => c.Order)
            .Include(c => c.AssignedSalesperson)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (CurrentCase == null)
        {
            return NotFound("Nie znaleziono zgłoszenia o podanym numerze.");
        }

        return Page();
    }
    public async Task<IActionResult> OnGetGetCommentsAsync(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return new JsonResult(new { error = "Użytkownik niezalogowany" }) { StatusCode = 401 };
        }

        // 2. Budujemy zapytanie wyciągające płaską listę komentarzy dla konkretnej sprawy
        var commentsData = await _db.SalesCases
            .Where(sc => sc.Id == id)
            .SelectMany(sc => sc.ActivityLogs)
            .OrderByDescending(log => log.SentAt)
            .Select(log => new
            {
                messageContent = log.MessageContent,
                sentAt = log.SentAt,
                direction = log.Direction.ToString(), // Enum zamieniamy na tekst (np. "INTERNAL")

                // Pobieramy nazwę nadawcy, jeśli istnieje (zabezpieczenie przed null)
                senderName = log.Sender != null ? log.Sender.UserName : "System"
            })
            .ToListAsync();

        // 3. Zwracamy jako JSON
        return new JsonResult(commentsData);
    }
    public async Task<IActionResult> OnPostSaveCommentAsync(int id, [FromBody] Payload payload)
    {
        if (id <= 0)
        {
            return new JsonResult(new { error = "Brak identyfikatora sprawy." }) { StatusCode = 400 };
        }

        if (payload == null || string.IsNullOrWhiteSpace(payload.MessageContent))
        {
            return new JsonResult(new { error = "Treść wiadomości nie może być pusta." }) { StatusCode = 400 };
        }
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return new JsonResult(new { error = "Użytkownik niezalogowany" }) { StatusCode = 401 };
        }

        var direction = payload.IsPrivateMessage
            ? MessageDirectionEnum.INTERNAL
            : MessageDirectionEnum.OUTBOUND;

        var comment = new CustomerCommunication
        {
            SalesCaseId = id,
            MessageContent = payload.MessageContent,
            Direction = direction,
            SenderId = currentUser.Id,
            IsPrivateMessage = payload.IsPrivateMessage,

            SentAt = DateTime.UtcNow
        };
        _db.CustomerCommunications.Add(comment);
        await _db.SaveChangesAsync();

        return new JsonResult(new { success = true, message = "Komentarz został zapisany." });
    }

    public async Task<IActionResult> OnGetLoadSalespeopleAsync()
    {
        var customers = await _salespeopleHelper.GetSalespeople();

        return new JsonResult(customers);
    }
    public async Task<IActionResult> OnPostChangeSalesmanAsync(int id, [FromBody] ChangeSalesmanDto input)
    {
        if (input == null || input.SalespersonId <= 0)
        {
            return new JsonResult(new { success = false, error = "Nieprawidłowe dane." }) { StatusCode = 400 };
        }

        var salesCase = await _db.SalesCases.FindAsync(id);
        if (salesCase == null)
        {
            return new JsonResult(new { success = false, error = "Nie znaleziono zgłoszenia." }) { StatusCode = 404 };
        }

        // Aktualizacja handlowca
        salesCase.AssignedSalespersonId = input.SalespersonId;

        await _db.SaveChangesAsync();

        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostCreateOfferAsync()
    {
        Guid guid = Guid.NewGuid(); //temp offer name

        var newOffer = new Offer
        {
            OfferFriendlyName = guid.ToString(),
            Status = OfferStatusEnum.DRAFT,
            SalesCaseId = Id
        };
        _db.Offers.Add(newOffer);
        await _db.SaveChangesAsync();

        newOffer.OfferFriendlyName = $"OFF-{DateTime.Now.Year}-{newOffer.Id:D4}";

        await _db.SaveChangesAsync();

        return new JsonResult(new { success = true });
    }

    

    // Prosta klasa DTO na dole pliku do złapania JSONa
    public class ChangeSalesmanDto
    {
        public int SalespersonId { get; set; }
    }

    public class Payload{
        public string MessageContent { get; set; }
        public bool IsPrivateMessage { get; set; }
    }
}
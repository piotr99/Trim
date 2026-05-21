using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trim.DbContext;
using Trim.Models;
using Trim.Models.DTOs;

namespace Trim.Services
{
    // 1. Interfejs (Kontrakt) - To jego użyjesz do mockowania w testach xUnit!
    public interface ICaseService
    {
        Task<SalesCase> GetCaseDetailsAsync(int id);
        Task<List<CaseDTO>> GetCommentsAsync(int id);
        Task<bool> UpdateDescriptionAsync(int id, string description);
        Task<bool> ChangeSalesmanAsync(int id, int salespersonId);
        Task<bool> CreateOfferAsync(int id);
        Task<bool> SaveCommentAsync(SaveCommentDTO commentDTO, int userId);
    }

    // 2. Właściwa implementacja logiki
    public class CaseService : ICaseService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CaseService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public CaseService(ApplicationDbContext db, ILogger<CaseService> logger, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<SalesCase> GetCaseDetailsAsync(int id)
        {
            _logger.LogInformation("Pobieranie szczegółów sprawy o ID: {CaseId}", id);

            // Pobieramy dane z bazy. Używamy tu AsNoTracking, jeśli w widoku OnGet tylko wyświetlamy dane.
            return await _db.SalesCases
                .Include(c => c.Customer)
                .Include(c => c.Offer)
                .Include(c => c.Order)
                .Include(c => c.AssignedSalesperson)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<CaseDTO>> GetCommentsAsync(int id)
        {
            try
            {
            var comments = await _db.SalesCases
                .AsNoTracking()
                .Where(sc => sc.Id == id)
                .SelectMany(sc => sc.ActivityLogs)
                .OrderByDescending(log => log.SentAt)
                .Select(log => new CaseDTO
                {
                    MessageContent = log.MessageContent,
                    SentAt = log.SentAt,
                    Direction = log.Direction.ToString(),
                    SenderName = log.Sender != null ? log.Sender.UserName : "System",
                    IsPrivate = log.IsPrivateMessage
                })
                .ToListAsync();
                return comments;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Nie udało się pobrać komentarzy dla sprawy: {CaseId}", id);
                return new List<CaseDTO>();
            }
        }

        public async Task<bool> UpdateDescriptionAsync(int id, string description)
        {
            _logger.LogInformation("Aktualizacja opisu dla sprawy: {CaseId}", id);

            try
            {
                var salesCase = await _db.SalesCases.FindAsync(id);
                if (salesCase == null)
                {
                    _logger.LogWarning("Nie znaleziono sprawy {CaseId} podczas próby aktualizacji opisu.", id);
                    return false; // Zwracamy false, bo nie znaleziono rekordu
                }

                salesCase.Description = description;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Pomyślnie zaktualizowano opis dla sprawy: {CaseId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił błąd zapisu do bazy podczas aktualizacji opisu dla sprawy {CaseId}", id);
                throw; // Wyrzucamy błąd wyżej, żeby PageModel (kontroler) go złapał w swoim try-catch
            }
        }

        public async Task<bool> ChangeSalesmanAsync(int id, int salespersonId)
        {
            _logger.LogInformation("Zmiana przypisanego handlowca w sprawie {CaseId} na użytkownika {SalespersonId}", id, salespersonId);

            try
            {
                var salesCase = await _db.SalesCases.FindAsync(id);
                if (salesCase == null)
                {
                    _logger.LogWarning("Nie znaleziono sprawy {CaseId} podczas próby zmiany handlowca.", id);
                    return false;
                }

                salesCase.AssignedSalespersonId = salespersonId;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Pomyślnie zmieniono handlowca w sprawie {CaseId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas przypisywania handlowca {SalespersonId} do sprawy {CaseId}", salespersonId, id);
                throw;
            }
        }

        public async Task<bool> CreateOfferAsync(int id)
        {
            _logger.LogInformation("Rozpoczęto proces tworzenia nowej oferty dla sprawy {CaseId}", id);

            try
            {
                // Tworzenie z tymczasowym GUIDem (dokładnie jak w Twojej logice biznesowej)
                Guid tempGuid = Guid.NewGuid();

                var newOffer = new Offer
                {
                    OfferFriendlyName = tempGuid.ToString(),
                    Status = OfferStatusEnum.DRAFT,
                    SalesCaseId = id
                };

                _db.Offers.Add(newOffer);
                await _db.SaveChangesAsync(); // Zapis, żeby wygenerować bazodanowe ID

                // Nadpisanie tymczasowego ID eleganckim numerem na podstawie przyznanego ID
                newOffer.OfferFriendlyName = $"OFF-{DateTime.Now.Year}-{newOffer.Id:D4}";
                await _db.SaveChangesAsync();

                _logger.LogInformation("Pomyślnie utworzono ofertę {OfferFriendlyName} dla sprawy {CaseId}", newOffer.OfferFriendlyName, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas generowania nowej oferty dla sprawy {CaseId}", id);
                throw;
            }
        }

        public async Task<bool> SaveCommentAsync(SaveCommentDTO commentDTO, int userId)
        {
            try
            {
                _logger.LogInformation("Rozpoczęto dodawanie komentarza dla sprawy {CaseId}. Dane: {@CommentDTO}", commentDTO.CaseId, commentDTO); 
                CustomerCommunication newComment = new CustomerCommunication
                {
                    IsPrivateMessage = commentDTO.IsPrivateMessage,
                    MessageContent = commentDTO.MessageContent,
                    ReadBySalespersonAt = DateTime.UtcNow,
                    SalesCaseId = commentDTO.CaseId,
                    Direction = MessageDirectionEnum.OUTBOUND,
                    SenderId = userId

                };

                _db.CustomerCommunications.Add(newComment); // Zmienione na synchroniczne Add
                await _db.SaveChangesAsync(); // DODANO KRYTYCZNY AWAIT!

                // Logujemy szczegóły zapisanego rekordu (z wygenerowanym ID)
                _logger.LogInformation("Pomyślnie zapisano komentarz. Wygenerowane ID: {CommentId}", newComment.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Krytyczny błąd podczas zapisu komentarza dla sprawy {CaseId}", commentDTO?.CaseId);
                return false;
            }
        }
    }
}
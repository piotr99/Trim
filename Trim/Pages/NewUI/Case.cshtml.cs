using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Trim.Helpers;
using Trim.Models;
using Trim.Models.DTOs; // Pamiętaj o dodaniu namespace'u dla swoich DTO
using Trim.Services;
using Trim.Services.AI;

namespace Trim.Pages.NewUI
{
    public class CaseModel : PageModel
    {
        private readonly ICaseService _caseService;
        private readonly ISalespeopleHelper _salespeopleHelper;
        private readonly IAiService _aiService;
        private readonly ILogger<CaseModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        // Wstrzykujemy wyłącznie serwisy biznesowe, żadnej bazy danych!
        public CaseModel(
            ICaseService caseService,
            ISalespeopleHelper salespeopleHelper,
            IAiService aiService,
            ILogger<CaseModel> logger,
            UserManager<ApplicationUser> userManager)
        {
            _caseService = caseService;
            _salespeopleHelper = salespeopleHelper;
            _aiService = aiService;
            _logger = logger;
            _userManager = userManager;
        }

        // Właściwość, przez którą przekażemy dane do widoku HTML (Razor)
        public SalesCase CurrentCase { get; set; }

        // Jedno źródło prawdy dla ID z paska adresu URL
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Zabezpieczenie przed brakiem ID
            if (Id <= 0)
            {
                return RedirectToPage("/NewUI/home");
            }

            // Delegujemy pobieranie sprawy do serwisu
            CurrentCase = await _caseService.GetCaseDetailsAsync(Id);

            if (CurrentCase == null)
            {
                _logger.LogWarning("Użytkownik próbował wejść w nieistniejącą sprawę o ID: {CaseId}", Id);
                return NotFound("Nie znaleziono zgłoszenia o podanym numerze.");
            }

            return Page();
        }

        public async Task<IActionResult> OnGetGetCommentsAsync(int id)
        {
            try
            {
                var commentsData = await _caseService.GetCommentsAsync(id);
                return new JsonResult(commentsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ładowania komentarzy dla sprawy {CaseId}", id);
                return new JsonResult(new { error = "Nie udało się załadować komentarzy." }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostSaveCommentAsync([FromBody] SaveCommentDTO commentDTO) // Dodano [FromBody], jeśli używasz fetch w JS
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                // Prawidłowa walidacja - dodajemy sprawdzenie stringa
                if (commentDTO == null || string.IsNullOrWhiteSpace(commentDTO.MessageContent) || commentDTO.CaseId <= 0)
                {
                    _logger.LogWarning("Odrzucono próbę zapisu komentarza z powodu brakujących danych: {@CommentDTO}", commentDTO);

                    // DODANO RETURN! Zwykły, czytelny tekst dla użytkownika.
                    return new JsonResult(new { success = false, error = "Treść komentarza i przypisanie do sprawy są wymagane." }) { StatusCode = 400 };
                }

                bool isSuccess = await _caseService.SaveCommentAsync(commentDTO, int.Parse(userId));

                // Obsługa sytuacji, gdy serwis zaraportował błąd (zwrócił false)
                if (!isSuccess)
                {
                    return new JsonResult(new { success = false, error = "Nie udało się zapisać komentarza w bazie danych." }) { StatusCode = 500 };
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                // DODANO LOGOWANIE BŁĘDU Z INTERFEJSU!
                _logger.LogError(ex, "Wystąpił nieoczekiwany błąd serwera podczas wywołania OnPostSaveCommentAsync");
                return new JsonResult(new { success = false, error = "Wystąpił wewnętrzny błąd systemu." }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostUpdateDescriptionAsync(int id, [FromBody] UpdateDescriptionDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Description))
                {
                    return new JsonResult(new { success = false, error = "Opis nie może być pusty." }) { StatusCode = 400 };
                }

                bool isSuccess = await _caseService.UpdateDescriptionAsync(id, request.Description.Trim());

                if (!isSuccess)
                {
                    return new JsonResult(new { success = false, error = "Nie znaleziono sprawy w systemie." }) { StatusCode = 404 };
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił błąd w interfejsie podczas aktualizacji opisu dla sprawy {CaseId}", id);
                return new JsonResult(new { success = false, error = "Wystąpił nieoczekiwany błąd systemu." }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnGetLoadSalespeopleAsync()
        {
            try
            {
                var salespeople = await _salespeopleHelper.GetSalespeople();
                return new JsonResult(salespeople);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd w interfejsie podczas ładowania listy handlowców.");
                return new JsonResult(new { error = "Błąd pobierania danych." }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostChangeSalesmanAsync(int id, [FromBody] ChangeSalesmanDto input)
        {
            try
            {
                if (input == null || input.SalespersonId <= 0)
                {
                    return new JsonResult(new { success = false, error = "Nieprawidłowe dane handlowca." }) { StatusCode = 400 };
                }

                bool isSuccess = await _caseService.ChangeSalesmanAsync(id, input.SalespersonId);

                if (!isSuccess)
                {
                    return new JsonResult(new { success = false, error = "Nie znaleziono zgłoszenia do aktualizacji." }) { StatusCode = 404 };
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd w interfejsie podczas zmiany handlowca dla sprawy {CaseId}", id);
                return new JsonResult(new { success = false, error = "Błąd systemu." }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostCreateOfferAsync(int id)
        {
            try
            {
                // Tutaj sprawdzamy, czy przekazano poprawne ID w żądaniu POST
                int targetId = id > 0 ? id : Id;

                if (targetId <= 0)
                {
                    return new JsonResult(new { success = false, error = "Brak ID sprawy." }) { StatusCode = 400 };
                }

                await _caseService.CreateOfferAsync(targetId);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd interfejsu przy tworzeniu oferty dla sprawy {CaseId}", id);
                return new JsonResult(new { success = false, error = "Nie udało się wygenerować oferty." }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostGenerateAiSummaryAsync(int id)
        {
            try
            {
                // Wywołujemy Twój serwis AI, przekazując ID sprawy
                var aiResponse = await _aiService.SummarizeSalesCaseAsync(id);

                return new JsonResult(new { success = true, summary = aiResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Krytyczny błąd interfejsu podczas generowania podsumowania AI dla sprawy {CaseId}", id);
                return new JsonResult(new { success = false, error = "Serwer napotkał problem podczas łączenia z AI." }) { StatusCode = 500 };
            }
        }
    }
}
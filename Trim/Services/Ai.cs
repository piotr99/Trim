using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Trim.DbContext;

namespace Trim.Services.AI
{
    public interface IAiService
    {
        Task<string> SummarizeSalesCaseAsync(int id);
        Task<bool> IsOllamaConnectedAsync();
    }

    public class AiService : IAiService
    {
        private readonly IChatClient _chatClient;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AiService> _logger;

        public AiService(IChatClient chatClient, ApplicationDbContext db, IConfiguration config, ILogger<AiService> logger)
        {
            _chatClient = chatClient;
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task<string> SummarizeSalesCaseAsync(int id)
        {
            // 1. Zawsze logujemy START ważnej operacji biznesowej
            _logger.LogInformation("Rozpoczęto generowanie podsumowania AI dla sprawy {SalesCaseId}.", id);

            try
            {
                bool ollamaConnected = await IsOllamaConnectedAsync();
                if (!ollamaConnected)
                {
                    _logger.LogWarning("Próba podsumowania sprawy {SalesCaseId} odrzucona. Brak połączenia z serwerem Ollama.", id);
                    return "Brak połączenia z lokalnym modelem AI. Nie można wygenerować podsumowania.";
                }

                var salesCase = await _db.SalesCases
                    .AsNoTracking()
                    .Include(sc => sc.ActivityLogs)
                        .ThenInclude(a => a.Sender)
                    .Include(sc => sc.Offer)
                        .ThenInclude(o => o.Vehicles)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (salesCase == null)
                {
                    // 2. Poprawiony błąd formatowania zmiennych
                    _logger.LogWarning("Anulowano generowanie AI. Nie znaleziono w bazie sprawy o ID: {SalesCaseId}.", id);
                    return "Brak opisu sprawy do podsumowania.";
                }

                var dataForAi = new
                {
                    salesCase.Title,
                    salesCase.Description,
                    Status = salesCase.Status.ToString(),
                    Logs = salesCase.ActivityLogs.Select(log => new
                    {
                        Sender = log.Sender?.NormalizedEmail,
                        Message = log.MessageContent
                    }).ToList(),
                    Vehicles = salesCase.Offer?.Vehicles.Select(v => v.Name).ToList()
                };

                string jsonDescription = JsonSerializer.Serialize(dataForAi, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var prompt = $@"Jesteś wulgarnym, piszącym wierszem piratem pomagającym w systemie CRM. 
                Przeanalizuj poniższe dane zgłoszenia (w formacie JSON) i streść całą sprawę w maksymalnie ośmiu zdaniach.
                Skup się na temacie sprawy, ustaleniach z logów oraz zaproponowanych pojazdach.

                Dane sprawy:
                {jsonDescription}";

                _logger.LogInformation("Wysyłanie zapytania do modelu LLM dla sprawy {SalesCaseId}...", id);

                var response = await _chatClient.GetResponseAsync(prompt); // Zmieniono na CompleteAsync zgodnie ze standardem

                // 3. Logowanie sukcesu
                _logger.LogInformation("Pomyślnie wygenerowano podsumowanie AI dla sprawy {SalesCaseId}.", id);

                return response.ToString();
            }
            catch (Exception ex)
            {
                // 4. Jeśli cokolwiek wybuchnie (baza, deserializacja, sieć), łapiemy to i przekazujemy 'ex' jako PIERWSZY argument!
                _logger.LogError(ex, "Wystąpił krytyczny błąd podczas podsumowywania sprawy {SalesCaseId}.", id);
                return "Wystąpił nieoczekiwany błąd systemu podczas komunikacji z AI.";
            }
        }

        public async Task<bool> IsOllamaConnectedAsync()
        {
            var ollamaUrl = _config["Ollama:BaseUrl"] ?? "http://localhost:11434";

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await httpClient.GetAsync(ollamaUrl);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) // Łapiemy obiekt wyjątku
            {
                // Rejestrujemy błąd podłączenia, ale nie blokujemy aplikacji
                _logger.LogWarning(ex, "Serwer Ollama pod adresem {OllamaUrl} nie odpowiedział w oczekiwanym czasie lub odrzucił połączenie.", ollamaUrl);
                return false;
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Helpers
{
    public interface IOrderHelper
    {
        Task<bool> CreateOffer(int caseId);
    }
    public class OrderHelper : IOrderHelper
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OrderHelper> _logger;
        public OrderHelper(ApplicationDbContext db, ILogger<OrderHelper> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> CreateOffer(int caseId)
        {
            if (caseId <= 0)
            {
                _logger.LogWarning("Nieprawidłowe ID sprawy: {CaseId}. Przerwano tworzenie zamówienia.", caseId);
                return false;
            }

            _logger.LogInformation("Rozpoczęto tworzenie zamówienia dla sprawy {CaseId}", caseId);

            try
            {
                var currentCase = await _db.SalesCases
                    .Include(sc => sc.Offer)
                        .ThenInclude(o => o.Vehicles)
                    .FirstOrDefaultAsync(sc => sc.Id == caseId);

                if (currentCase == null)
                {
                    _logger.LogWarning("Nie można utworzyć zamówienia. Sprawa o ID {CaseId} nie istnieje w bazie.", caseId);
                    return false;
                }

                if (currentCase.Offer == null)
                {
                    _logger.LogWarning("Sprawa {CaseId} nie posiada przypisanej oferty. Anulowano tworzenie zamówienia.", caseId);
                    return false;
                }

                string orderNumber = $"ORD-{DateTime.Now.Year}-{currentCase.Id:D4}";

                Order newOrder = new Order
                {
                    OrderNumber = orderNumber,
                    Status = OrderStatusEnum.NEW,
                    FinalPrice = currentCase.Offer.FinalPrice,
                    SalesCaseId = caseId,
                    Vehicles = currentCase.Offer.Vehicles.ToList()
                };

                _db.Orders.Add(newOrder);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Pomyślnie utworzono zamówienie {OrderNumber} dla sprawy {CaseId}", newOrder.OrderNumber, caseId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Krytyczny błąd podczas tworzenia zamówienia dla sprawy {CaseId}.", caseId);
                return false;
            }
        }
    }

}

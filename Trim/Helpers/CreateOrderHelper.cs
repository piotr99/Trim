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
        public OrderHelper(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> CreateOffer(int caseId)
        {
            Guid guid = new Guid();
            var currentCase = await _db.SalesCases
            .Include(sc => sc.Offer)
                .ThenInclude(o => o.Vehicles)
            .Where(sc => sc.Id == caseId)
            .FirstOrDefaultAsync();
            Order newOrder = new Order
            {
                OrderNumber = $"ORD-{DateTime.Now.Year}-{currentCase.Id:D4}",
                Status = OrderStatusEnum.NEW,
                FinalPrice = currentCase.Offer.FinalPrice,
                SalesCaseId = caseId,

                //ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
                Vehicles = currentCase.Offer.Vehicles.ToList()
 
            };
            _db.Orders.Add(newOrder);
            await _db.SaveChangesAsync();
            //newCase.CaseNumber = $"CASE-{DateTime.Now.Year}-{newCase.Id:D4}";

            return true;
        }
    }

}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var providedApiKey = Request.Headers["X-API-KEY"].FirstOrDefault();

            var validApiKey = "cokolwiek123";

            if (providedApiKey != validApiKey)
            {
                return Unauthorized(new { error = "Nieprawidłowy lub brakujący klucz API." });
            }

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { error = $"Nie znaleziono zamówienia o ID {id}" });
            }

            order.Status = request.NewStatus;

            await _db.SaveChangesAsync();

            // Zwracamy odpowiedź do systemu producenta
            return Ok(new
            {
                success = true,
                message = "Status zaktualizowany pomyślnie.",
                orderId = order.Id,
                newStatus = order.Status.ToString()
            });
        }
    }

    // Klasa DTO używana tylko do odbierania danych z tego konkretnego zapytania
    public class UpdateStatusRequest
    {
        public OrderStatusEnum NewStatus { get; set; }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; // Niezbędne dla metody OfType()
using System.Threading.Tasks;
using Trim.Models;

namespace Trim.Helpers
{
    public interface ISalespeopleHelper
    {
        Task<List<Salesperson>> GetSalespeople();
    }

    public class SalespeopleHelper : ISalespeopleHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SalespeopleHelper> _logger;

        public SalespeopleHelper(UserManager<ApplicationUser> userManager, ILogger<SalespeopleHelper> logger) 
        {
            _userManager = userManager;
                _logger = logger;
        }

        public async Task<List<Salesperson>> GetSalespeople()
        {
            _logger.LogInformation("Rozpoczęto pobieranie listy sprzedawców z bazy danych.");

            try
            {
                var salespeople = await _userManager.Users
                    .AsNoTracking()
                    .OfType<Salesperson>()
                    .ToListAsync();

                _logger.LogInformation("Pobrano {Count} sprzedawców z bazy.", salespeople.Count);

                return salespeople;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Krytyczny błąd podczas pobierania listy sprzedawców.");

                return new List<Salesperson>();
            }
        }
    }
}
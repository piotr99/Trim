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

        public SalespeopleHelper(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<Salesperson>> GetSalespeople()
        {
            // Metoda OfType<> mówi EF Core, aby wyciągnął tylko rekordy typu Salesperson
            return await _userManager.Users
                .OfType<Salesperson>()
                .ToListAsync();
        }
    }
}
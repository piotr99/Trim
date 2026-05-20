using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.NewUI.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;


        public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public Order CurrentOrder { get; set; } = default!;
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            CurrentOrder = await _db.Orders
                .AsNoTracking()
                .Include(o => o.SalesCase)
                .Where(o => o.SalesCaseId == id)
                .Include(o => o.Vehicles)
                    .ThenInclude(v => v.Configuration)
                .FirstOrDefaultAsync();

            if (CurrentOrder == null)
            {
                return NotFound();
            }

            return Page();
        }
    } 
    
}

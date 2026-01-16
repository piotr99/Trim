using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;
using Microsoft.AspNetCore.Identity;


namespace Trim.Pages.Salesperson;

public class ManageVehicles : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageVehicles(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    } 

    public List<Vehicle> Vehicles { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? VehicleId { get; set; }

    public int? SelectedVehicleId => VehicleId;
    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }
    public bool HasPrev => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public async Task OnGetAsync()
    {
        if (CurrentPage < 1) CurrentPage = 1;
        
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return;
        
        IQueryable<Vehicle> q = _db.Vehicles.AsNoTracking();
        
        bool isSalesperson = await _userManager.IsInRoleAsync(currentUser, "Salesperson");
        if (isSalesperson)
        {
            //q = q.Where(c => c.SalespersonId == uid);
        }

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            var f = Filter.Trim();
            q = q.Where(a => a.Name.Contains(f) || a.Vin.Contains(f));
        }
        var totalCount = await q.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        Vehicles = await q.OrderBy(v => v.Name).ToListAsync();
    }

    // AJAX: zwraca HTML partiala
    public async Task<PartialViewResult> OnPostVehicleDetailsPartialAsync([FromForm] int vehicleId)
    {
        // jeśli partial ma brć samo Id -> możesz zwrócić tylko Ida
        // ale zwykle lepiej przekazać cały obiekt:
        var vehicle = await _db.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        return Partial("Salesperson/Partial/_VehicleDetails", vehicle);
    }
}
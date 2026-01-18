using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;
using Microsoft.AspNetCore.Identity;

namespace Trim.Pages.Salesperson;

public class VehicleConfigurator : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public VehicleConfigurator(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // jeśli tworzysz pojazd dla konkretnego klienta:
    [BindProperty(SupportsGet = true)]
    public int? customerId { get; set; }

    [BindProperty]
    public Vehicle NewVehicle { get; set; } = new();

    [BindProperty]
    public VehicleConfiguration NewConfiguration { get; set; } = new();

    // dropdown opcji
    public Option NewOption { get; set; } = new();

    public async Task OnGetAsync()
    {
    }

    public async Task<IActionResult> OnPostCreateVehicleAsync()
    {
        await OnGetAsync(); // potrzebne, gdy wracasz Page() przy błędach

        if (!ModelState.IsValid)
            return Page();

        // opcjonalnie przypisz klienta, jeśli przekazujesz customerId w query
        if (customerId.HasValue)
            NewVehicle.CustomerId = customerId.Value;
        
        NewVehicle.Configuration = new VehicleConfiguration
        {
            Size = NewConfiguration.Size,
            Engine = NewConfiguration.Engine,
            Gerabox = NewConfiguration.Gerabox,
            Interior = NewConfiguration.Interior,
            Drivetrain = NewConfiguration.Drivetrain,
            Option = new Option
            {
                Description = NewOption.Description,
                Price = NewOption.Price
            }
        };

        _db.Vehicles.Add(NewVehicle);
        await _db.SaveChangesAsync();

        // docelowa strona zależy od Ciebie:
        // - jeśli tworzysz dla klienta -> lista pojazdów klienta
        // - jeśli globalnie -> lista wszystkich pojazdów
        if (customerId.HasValue)
            return RedirectToPage("/Salesperson/CustomerVehicles", new { customerId = customerId.Value });

        return RedirectToPage("/Salesperson/ManageVehicles", new { Filter = NewVehicle.Name });
    }
}

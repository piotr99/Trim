using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Trim.Pages.Salesperson.Partial;

public class CreateOffer : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateOffer(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    } 

    [BindProperty(SupportsGet = true)]
    public int customerId { get; set; }   //otrzymuję z asp-route-customerId="@Model.Id"
    
    [BindProperty]
    public Offer NewOffer { get; set; } = new();

    [BindProperty]
    public ICollection<Vehicle> NewVehicles { get; set; } = new List<Vehicle>();

    [BindProperty]
    public OfferVersion NewOfferVersion { get; set; } = new();
    [BindProperty]
    public VehicleConfiguration NewVehicleConfiguration { get; set; } = new();

    [BindProperty]
    public PdfDocument NewPdfDocument { get; set; } = new();
    [BindProperty]
    public Option NewOption { get; set; } = new();
    
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostCreateOfferAsync()
    {


        var user = await _userManager.GetUserAsync(User);

        // 1) jeśli zalogowany Salesperson
        if (user != null && await _userManager.IsInRoleAsync(user, "Salesperson"))
        {
            NewOffer.SalespersonId = user.Id;
        }
        else
        {
            // 2) spróbuj wziąć handlowca z klienta
            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer?.SalespersonId != null)
            {
                NewOffer.SalespersonId = customer.SalespersonId.Value;
            }
            else
            {
                // 3) fallback: weź pierwszego handlowca z bazy (albo zwróć błąd)
                var fallbackSalespersonId = await _db.Users
                    .Where(u => EF.Property<string>(u, "UserType") == "Salesperson")
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                if (fallbackSalespersonId == 0)
                    return BadRequest("Brak handlowca w systemie.");

                NewOffer.SalespersonId = fallbackSalespersonId;
            }
        }
        //Określ wersję
        NewOfferVersion.VersionNumber = 1;
        
        // buduj graf encji (prościej i bezpieczniej)
        NewOffer.CustomerId = customerId;
        foreach (var vehicle in NewVehicles)
        {
            vehicle.CustomerId = customerId;
            //NewOffer.Vehicle = vehicle; TODO
        }
        

        NewOffer.Versions.Add(NewOfferVersion);
        NewOffer.PdfDocument = NewPdfDocument;
        if (!ModelState.IsValid)
            return Page();
        _db.Offers.Add(NewOffer);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Salesperson/CustomerOffers", new { customerId });
    }


}
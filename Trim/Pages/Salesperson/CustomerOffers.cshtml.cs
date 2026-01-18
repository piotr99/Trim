using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.Salesperson;

public class CustomerOffers : PageModel
{
    private readonly ApplicationDbContext _db;
    public CustomerOffers(ApplicationDbContext db) => _db = db;

    public List<Offer> Offers { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int customerId { get; set; }
    public Customer? Customer { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }
    public Offer? SelectedOffer { get; set; } 
    public Offer? NewOffer {get; set;}

    public async Task<IActionResult> OnGetAsync()
    {
        if (customerId <= 0)
            return RedirectToPage("/Salesperson/CustomerOffers");

        Customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (Customer == null)
            return RedirectToPage("/Salesperson/CustomerOffers");
        

        IQueryable<Offer> q = _db.Offers
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OfferVehicles)
            .ThenInclude(ov => ov.Vehicle);

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            var f = Filter.Trim();
            q = q.Where(o => EF.Functions.Like(o.OfferFriendlyName, $"%{f}%"));
        }

        Offers = await q.OrderByDescending(o => o.Id).ToListAsync();
        return Page();
    }

    public async Task<PartialViewResult> OnPostOfferDetailsPartialAsync([FromForm] int offerId)
    {
        var offer = await _db.Offers.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == offerId);
        return Partial("Salesperson/Partial/_OfferDetails", offer);
    }

}
//adm-spl_pietrzakp@scania.pl #@nyo9eINc*T0V2q6^0ee
//scaniaazureservices.onmicrosoft.com\piotr.pietrzak Paprykacebulaadmin4%
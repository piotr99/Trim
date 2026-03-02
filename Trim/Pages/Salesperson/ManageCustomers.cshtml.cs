using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Trim.DbContext;
using Trim.Models;
using Trim.Models.ViewModels;

namespace Trim.Pages.Salesperson;

public class ManageCustomers : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageCustomers(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<Customer> Customers { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CustomerId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }
    public bool HasPrev => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    [BindProperty]
    public CustomerCreateVm CustomerCreateVm { get; set; } = new();

    private async Task<List<SelectListItem>> GetSalespeopleSelectAsync() 
        => await _userManager.Users.AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.UserName
            })
            .ToListAsync();

    public async Task OnGetAsync()
    {
        if (CurrentPage < 1) CurrentPage = 1;

        var q = _db.Customers.AsNoTracking().AsQueryable();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null)
        {
            var isSalesperson = await _userManager.IsInRoleAsync(currentUser, "Salesperson");
            if (isSalesperson)
            {
                var uid = currentUser.Id;
                q = q.Where(c => c.SalespersonId == uid);
            }
        }
        
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            var f = Filter.Trim();
            q = q.Where(c =>
                c.FirstName.Contains(f) ||
                c.LastName.Contains(f) ||
                (c.CompanyName != null && c.CompanyName.Contains(f)) ||
                (c.Email != null && c.Email.Contains(f)));
        }

        var totalCount = await q.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        Customers = await q
            .OrderBy(c => c.FirstName)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<PartialViewResult> OnPostCustomerDetailsPartialAsync([FromForm] int customerId)
    {
        var customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        return Partial("Salesperson/Partial/_CustomerDetails", customer);
    }

    public async Task<PartialViewResult> OnPostCustomerCreateFormPartialAsync()
    {
        var vm = new CustomerCreateVm
        {
            Customer = new Customer(),
            Salespeople = await GetSalespeopleSelectAsync()
        };

        return Partial("Salesperson/Partial/_CustomerCreateForm", vm);
    }

    public async Task<IActionResult> OnPostCreateCustomerAsync()
    {
        CustomerCreateVm.Salespeople = await GetSalespeopleSelectAsync();

        if (!ModelState.IsValid)
            return Partial("Salesperson/Partial/_CustomerCreateForm", CustomerCreateVm);

        _db.Customers.Add(CustomerCreateVm.Customer);
        await _db.SaveChangesAsync();

        // ✅ pokaż szczegóły w rightPanel
        return RedirectToPage("/Salesperson/ManageCustomers");
    }

}

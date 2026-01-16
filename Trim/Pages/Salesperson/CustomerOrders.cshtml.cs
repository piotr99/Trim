using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.Salesperson;

public class CustomerOrders : PageModel
{
    private readonly ApplicationDbContext _db;
    public CustomerOrders(ApplicationDbContext db) => _db = db;
    
    public List<Order> Orders { get; set; }
    [BindProperty(SupportsGet = true)]
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int customerId { get; set; } 


    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == CustomerId);

        if (CustomerId <= 0)
            return RedirectToPage("/Salesperson/ManageCustomers");
        
        var q = _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == Customer.Id);

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            var f = Filter.Trim();
            q = q.Where(o => o.OrderNumber.Contains(f));
        }

        Orders = await q
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        return Page();
    }
    public async Task<PartialViewResult> OnPostOrderDetailsPartialAsync([FromForm] int orderId)
    {
        var order = await _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
        return Partial("Salesperson/Partial/_OrderDetails", order);
    }
    
}
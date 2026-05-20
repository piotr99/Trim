using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages;

public class Home : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    public Home(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<IActionResult> OnGet()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Redirect("/Account/Login");
        }
        bool isRegularUser = await _userManager.IsInRoleAsync(currentUser, "Customer");

        if (isRegularUser)
        {
            return Redirect("/Customer/Home");
        }
        else
        {
            return Redirect("/NewUI/Home");
        }
    }
}
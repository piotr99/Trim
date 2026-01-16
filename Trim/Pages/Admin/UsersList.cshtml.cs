using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trim.Models;

namespace Trim.Pages.Admin;

public class UsersList : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    // 1. DODANO KONSTRUKTOR - bez tego userManager i roleManager będą nullami
    public UsersList(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    [BindProperty]
    public string currentUserId { get; set; }
    public List<ApplicationUser> users { get; set; } = new();
    
    public void OnGet()
    {
        // Usuwamy "List<ApplicationUser>", aby przypisać wynik do właściwości klasy
        users = _userManager.Users
            .OrderBy(u => u.UserName)
            .ToList();
        currentUserId = _userManager.GetUserId(User); // Pobierz raz
    }
    public async Task<IActionResult> OnPostDeactivateAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
    
        if (user != null)
        {
            user.LockoutEnabled = false;
            var result = await _userManager.UpdateAsync(user);
        
            if (result.Succeeded)
            {
            }
        }

        return RedirectToPage(); // Odświeża stronę, by pokazać aktualny stan
    }

    public async Task<IActionResult> OnPostActivateAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user != null)
        {
            user.LockoutEnabled = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                
            }
            
        }
        return RedirectToPage();
    }
}
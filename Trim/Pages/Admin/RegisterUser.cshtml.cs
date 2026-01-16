using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trim.Models; // ApplicationUser

namespace Trim.Pages.Admin;

public class RegisterUserModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public RegisterUserModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public RegisterModel RegisterModel { get; set; } = new();

    [BindProperty]
    public string? RoleName { get; set; }

    public List<string> AllRoles { get; set; } = new();

    private async Task LoadRolesAsync()
    {
        AllRoles = await _roleManager.Roles
            .Select(r => r.Name!)
            .OrderBy(n => n)
            .ToListAsync();
    }

    public async Task OnGetAsync()
    {
        await LoadRolesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadRolesAsync();

        if (!ModelState.IsValid)
            return Page();

        // 1) Walidacja roli
        if (string.IsNullOrWhiteSpace(RoleName))
        {
            ModelState.AddModelError(nameof(RoleName), "Wybierz rolę.");
            return Page();
        }

        if (!await _roleManager.RoleExistsAsync(RoleName))
        {
            ModelState.AddModelError(nameof(RoleName), "Wybrana rola nie istnieje.");
            return Page();
        }

        // 2) Czy user już istnieje?
        var existing = await _userManager.FindByEmailAsync(RegisterModel.Email);
        if (existing != null)
        {
            ModelState.AddModelError(nameof(RegisterModel.Email), "Użytkownik o takim emailu już istnieje.");
            return Page();
        }

        // 3) Tworzenie usera + zapis pól profilowych
        var newUser = new ApplicationUser
        {
            UserName = RegisterModel.Email,
            Email = RegisterModel.Email,
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(newUser, RegisterModel.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }

        // 4) Przypisanie roli
        var roleResult = await _userManager.AddToRoleAsync(newUser, RoleName);
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            // opcjonalnie rollback - usuń usera jeśli rola się nie przypisała
            await _userManager.DeleteAsync(newUser);
            return Page();
        }

        // OK
        TempData["Success"] = $"Utworzono użytkownika {newUser.Email} z rolą {RoleName}.";
        return RedirectToPage("/Admin/UsersList");
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trim.Models;
using Trim.Models.ViewModels;

namespace Trim.Pages.Admin;

public class ManageUsers : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public ManageUsers(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public bool HasPrev => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public List<ApplicationUser> Users { get; set; } = new();

    // VM do tworzenia usera w partialu
    [BindProperty]
    public UserCreateVm UserCreateVm { get; set; } = new();
    public UserDetailsVM  UserDetailsVM { get; set; } = new();
    public EditUserVm EditUserVm { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (CurrentPage < 1) CurrentPage = 1;

        IQueryable<ApplicationUser> q = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            var f = Filter.Trim();
            q = q.Where(u =>
                u.UserName!.Contains(f) ||
                (u.Email != null && u.Email.Contains(f)) ||
                (u.FirstName != null && u.FirstName.Contains(f)) ||
                (u.LastName != null && u.LastName.Contains(f)));
        }

        var totalCount = await q.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        Users = await q
            .OrderBy(u => u.UserName)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<PartialViewResult> OnPostUserDetailsPartialAsync([FromForm] int userId)
    {
        var user = await _userManager.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        var roles = await _userManager.GetRolesAsync(user);
        UserDetailsVM = new UserDetailsVM
        {
            User = user,
            IsLocked = await _userManager.IsLockedOutAsync(user),
            Role = roles.FirstOrDefault(),
        };

        return Partial("Admin/Partial/_UserDetails", UserDetailsVM);
    }

    public async Task<PartialViewResult> OnPostUserCreateFormPartialAsync()
    {
        UserCreateVm = new UserCreateVm
        {
            Roles = await _roleManager.Roles.AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
                .ToListAsync()
        };

        return Partial("Admin/Partial/_UserCreateForm", UserCreateVm);
    }

    public async Task<IActionResult> OnPostCreateUserAsync()
    {
        // doładuj role na wypadek walidacji
        UserCreateVm.Roles = await _roleManager.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
            .ToListAsync();

        if (!ModelState.IsValid)
            return Partial("Admin/Partial/_UserCreateForm", UserCreateVm);

        var u = new ApplicationUser
        {
            UserName = UserCreateVm.Email,   // zwykle login = email
            Email = UserCreateVm.Email,
            EmailConfirmed = true,
            FirstName = UserCreateVm.FirstName,
            LastName = UserCreateVm.LastName
        };

        var create = await _userManager.CreateAsync(u, UserCreateVm.Password);
        if (!create.Succeeded)
        {
            foreach (var e in create.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return Partial("Admin/Partial/_UserCreateForm", UserCreateVm);
        }

        if (!string.IsNullOrWhiteSpace(UserCreateVm.RoleName))
        {
            var addRole = await _userManager.AddToRoleAsync(u, UserCreateVm.RoleName);
            if (!addRole.Succeeded)
            {
                foreach (var e in addRole.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                return Partial("Admin/Partial/_UserCreateForm", UserCreateVm);
            }
        }
        var roles = await _userManager.GetRolesAsync(u);

        UserDetailsVM = new UserDetailsVM
        {
            User = u,
            IsLocked = await _userManager.IsLockedOutAsync(u),
            Role = roles.FirstOrDefault(),
        };

        // zwróć HTML do rightPanel (AJAX)
        return Partial("Admin/Partial/_UserDetails", UserDetailsVM);
    }
    //lick user
    public async Task<IActionResult> OnPostEnableDisableUserAsync([FromForm] string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        
        bool isCurrentlyLocked = await _userManager.IsLockedOutAsync(user);

        if (isCurrentlyLocked)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(new DateTime(2099, 1, 1)));
        }
        
        await _userManager.UpdateSecurityStampAsync(user);
        
        var role = await _userManager.GetRolesAsync(user);
        UserDetailsVM = new UserDetailsVM
        {
            User = user,
            IsLocked = await _userManager.IsLockedOutAsync(user),
            Role = role.FirstOrDefault()
        };

        return Partial("Admin/Partial/_UserDetails", UserDetailsVM);
    }

    public async Task<IActionResult> OnPostEditUserAsync([FromForm] string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);

        var vm = new EditUserVm
        {
            UserId  = userId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            SelectedRole = userRoles.FirstOrDefault(),
            Roles = await _roleManager.Roles.AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name!
                })
                .ToListAsync(),
        };

        return Partial("Admin/Partial/_EditUser", vm);
    }

    public async Task<IActionResult> OnPostSaveUserAsync(EditUserVm model, [FromForm] string userId)
{
    ModelState.Remove("Password");
    if (!ModelState.IsValid)
    {
        // odbuduj Roles (select) i zwróć partial formularza
        model.Roles = await _roleManager.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
            .ToListAsync();

        return Partial("Admin/Partial/_EditUser", model);
    }

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound();

    // aktualizacja pól
    user.FirstName = model.FirstName;
    user.LastName  = model.LastName;
    user.Email     = model.Email;
    user.UserName  = model.Email; // jeśli u Ciebie username = email (opcjonalnie)

    var update = await _userManager.UpdateAsync(user);
    if (!update.Succeeded)
    {
        foreach (var err in update.Errors)
            ModelState.AddModelError(string.Empty, err.Description);

        model.Roles = await _roleManager.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
            .ToListAsync();

        return Partial("Admin/Partial/_EditUser", model);
    }

    // role
    var roles = await _userManager.GetRolesAsync(user);     // IList<string>
    var currentRole = roles.FirstOrDefault();              // string? (zakładamy 1 rolę)
    var newRole = string.IsNullOrWhiteSpace(model.SelectedRole) ? null : model.SelectedRole;

    if (currentRole != newRole)
    {
        if (currentRole != null)
            await _userManager.RemoveFromRoleAsync(user, currentRole);

        if (newRole != null)
            await _userManager.AddToRoleAsync(user, newRole);
    }
    var rolesAfter = await _userManager.GetRolesAsync(user);
    // zwróć widok szczegółów (albo JSON)
    UserDetailsVM = new UserDetailsVM
    {
        User = user,
        IsLocked = await _userManager.IsLockedOutAsync(user),
        Role = rolesAfter.FirstOrDefault()
    };

    return Partial("Admin/Partial/_UserDetails", UserDetailsVM);
}

    public async Task<IActionResult> OnPostCancelEditUserAsync([FromForm] string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        UserDetailsVM = new UserDetailsVM
        {
            User = user,
            IsLocked = await _userManager.IsLockedOutAsync(user),
            Role = roles.FirstOrDefault()
        };
        return Partial("Admin/Partial/_UserDetails", UserDetailsVM);
    }

}

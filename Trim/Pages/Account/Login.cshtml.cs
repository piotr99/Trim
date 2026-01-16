using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Trim.Models;
using System.ComponentModel.DataAnnotations;

namespace Trim.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager,  UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        

        public async Task OnGetAsync(string returnUrl = null)
        {
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Page("/Home") ?? "/Home";

            if (ModelState.IsValid)
            {
                // Główna metoda logująca:
                // lockoutOnFailure: true włącza blokadę konta po kilku błędnych próbach
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email, 
                    Input.Password, 
                    Input.RememberMe, 
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToPage("/Home");
                }
                
                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }
            }

            return Page();
        }
    }
}
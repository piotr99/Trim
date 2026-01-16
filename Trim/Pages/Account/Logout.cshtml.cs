using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Trim.Models;

namespace Trim.Pages.Account
{
    // Ignorujemy token Antiforgery tylko jeśli chcemy pozwolić na wylogowanie przez GET (niezalecane)
    // Ale my używamy POST, więc wszystko jest bezpieczne.
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Jeśli nie ma returnUrl, wróć na stronę główną
                return RedirectToPage("/Index");
            }
        }
    }
}
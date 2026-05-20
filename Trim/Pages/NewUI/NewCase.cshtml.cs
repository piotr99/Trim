using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trim.DbContext;
using Trim.Helpers;
using Trim.Models;

namespace Trim.Pages.NewUI
{
    public class NewCaseModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NewCaseModel(ApplicationDbContext db , UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public async Task<IActionResult> OnGetLoadCustomersAsync()
        {
            var customers = await _db.Customers
                .AsNoTracking()
                .Select(c => new
                {
                    id = c.Id,
                    displayName = string.IsNullOrEmpty(c.CompanyName)
                        ? c.FirstName + " " + c.LastName
                        : c.CompanyName + " (" + c.FirstName + " " + c.LastName + ")"
                })
                .OrderBy(c => c.displayName)
                .ToListAsync();

            return new JsonResult(customers);
        }
        public async Task<IActionResult> OnPostSaveCaseAsync([FromBody] CreateCaseDto input)
        {
            if (!ModelState.IsValid || input.CustomerId <= 0 || string.IsNullOrWhiteSpace(input.Title))
            {
                return new JsonResult(new { success = false, error = "Wypełnij wszystkie wymagane pola." }) { StatusCode = 400 };
            }

             var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == null || currentUser.Id == 0)
            {
                return new JsonResult(new { success = false });
            }
             int currentSalespersonId = currentUser.Id;

            // Utworzenie obiektu Case (SalesCase)
            var newCase = new SalesCase
            {
                Title = input.Title.Trim(),
                Description = input.Description?.Trim(),
                CustomerId = input.CustomerId,
                Status = SalesCaseStatusEnum.NEW, // Twój enum statusu
                CreatedAt = DateTime.Now,
                AssignedSalespersonId = currentSalespersonId 
            };

            Guid guid = Guid.NewGuid();
            newCase.CaseNumber = guid.ToString();

            _db.SalesCases.Add(newCase);
            await _db.SaveChangesAsync();

            // Wygenerowanie przyjaznego CaseNumber (np. CASE-2026-0015)
            // Musimy to zrobić po SaveChanges, aby mieć wygenerowane Id
            newCase.CaseNumber = $"CASE-{DateTime.Now.Year}-{newCase.Id:D4}";

            await _db.SaveChangesAsync();

            // Zwracamy sukces wraz z ID nowego obiektu, żeby JavaScript mógł przekierować użytkownika
            return new JsonResult(new { success = true, newCaseId = newCase.Id });
        }
    }

    // DTO (Data Transfer Object) pomagający odebrać dane JSON z AJAXa
    public class CreateCaseDto
    {
        public int CustomerId { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;

namespace Trim.Pages.NewUI.Vehicles
{
    public class ParkingLotVehiclesModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        public ParkingLotVehiclesModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            Vehicles = await _db.Vehicles
                .AsNoTracking()
                .Include(v => v.Offer)
                .Where(v => v.Offer == null || v.Offer.SalesCaseId == 0)
                .ToListAsync();
        }


        public async Task<IActionResult> OnPostSendToOfferAsync([FromBody] SendVehiclesToOfferDto request)
        {
            try
            {
                if (request == null || !request.SelectedVehicleIds.Any())
                {
                    return new JsonResult(new { success = false, error = "Nie wybrano żadnych pojazdów." }) { StatusCode = 400 };
                }


                // Szukamy oferty
                var targetOffer = await _db.Offers
                    .Include(o => o.Vehicles)
                    .FirstOrDefaultAsync(o => o.SalesCaseId == request.CaseId);

                if (targetOffer == null)
                {
                    return new JsonResult(new { success = false, error = $"Nie znaleziono oferty o podanym ID ${targetOffer}." }) { StatusCode = 404 };
                }

                // Szukamy wybranych pojazdów (dodatkowe zabezpieczenie upewniające się, że nikt ich w międzyczasie nie kupił)
                var vehiclesToMove = await _db.Vehicles
                    .Where(v => request.SelectedVehicleIds.Contains(v.Id))
                    .ToListAsync();

                foreach (var vehicle in vehiclesToMove)
                {
                    targetOffer.Vehicles.Add(vehicle);
                }

                await _db.SaveChangesAsync();

                // Zwracamy sukces oraz link, pod który JS ma przekierować handlowca
                return new JsonResult(new
                {
                    success = true,
                    redirectUrl = $"/NewUI/Offers/Details?id={request.CaseId}"
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = "Wystąpił błąd: " + ex.Message });
            }
        }

        public class SendVehiclesToOfferDto
        {
            public List<int> SelectedVehicleIds { get; set; } = new List<int>();
            public int CaseId { get; set; }
        }
    }
}
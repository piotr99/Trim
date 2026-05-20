using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trim.Models;

namespace Trim.Helpers
{
    public interface IUpdateOfferPricing
    {
        Task<List<decimal>> Update(List<Vehicle> vehicles);
    }

    public class UpdateOfferPricing : IUpdateOfferPricing
    {
        public async Task<List<decimal>> Update(List<Vehicle> vehicles)
        {
            decimal totalPrice = 0m;
            decimal totalDiscount = 0m; // Miejsce na przyszłą logikę wyliczania rabatów
            decimal totalBonus = 0m;
            decimal finalPrice = 0m;

            // Zabezpieczenie przed pustą listą
            if (vehicles != null && vehicles.Any())
            {
                foreach (var vehicle in vehicles)
                {
                    if (vehicle.Configuration != null)
                    {
                        var config = vehicle.Configuration;

                        // 1. Wyliczenie ceny pojazdu (Baza + Dodatki)
                        decimal vehiclePrice = config.Price + (config.AdditionalPrice ?? 0m);
                        totalPrice += vehiclePrice;

                        // 2. Wyliczenie bonusów (Stała kwota + Mnożnik np. 0.05 dla 5%)
                        decimal vehicleBonus = config.Bonus + (vehiclePrice * config.BonusMultiplier);
                        totalBonus += vehicleBonus;
                    }
                }
            }

            // 3. Obliczenie ceny ostatecznej dla całej oferty
            finalPrice = totalPrice - totalDiscount - totalBonus;

            // Zabezpieczenie przed ujemną ceną końcową (np. przy błędnie wpisanym bonusie)
            if (finalPrice < 0)
            {
                finalPrice = 0m;
            }

            // 4. Zwracamy ustandaryzowaną listę wartości
            // Indeks 0: Price | Indeks 1: Discount | Indeks 2: Bonus | Indeks 3: FinalPrice
            var result = new List<decimal>
            {
                totalPrice,
                totalDiscount,
                totalBonus,
                finalPrice
            };

            // Używamy Task.FromResult, ponieważ metoda jest synchroniczna w swoim wnętrzu,
            // ale implementuje asynchroniczny interfejs Task<T>.
            return await Task.FromResult(result);
        }
    }
}
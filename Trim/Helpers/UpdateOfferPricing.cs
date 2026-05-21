using Microsoft.Extensions.Logging;
using System;
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
        private readonly ILogger<UpdateOfferPricing> _logger;

        public UpdateOfferPricing(ILogger<UpdateOfferPricing> logger)
        {
            _logger = logger;
        }

        public async Task<List<decimal>> Update(List<Vehicle> vehicles)
        {
            if (vehicles == null || !vehicles.Any())
            {
                _logger.LogWarning("Próba przeliczenia cen dla pustej (lub nullowej) listy pojazdów. Zwrócono wartości zerowe.");
                return new List<decimal> { 0m, 0m, 0m, 0m };
            }

            _logger.LogInformation("Rozpoczęto przeliczanie cen dla {VehicleCount} pojazdów.", vehicles.Count);

            try
            {
                decimal totalPrice = 0m;
                decimal totalDiscount = 0m;
                decimal totalBonus = 0m;
                decimal finalPrice = 0m;

                foreach (var vehicle in vehicles)
                {
                    if (vehicle.Configuration != null)
                    {
                        var config = vehicle.Configuration;

                        decimal vehiclePrice = config.Price + (config.AdditionalPrice ?? 0m);
                        totalPrice += vehiclePrice;

                        // Wyliczenie bonusów
                        decimal vehicleBonus = config.Bonus + (vehiclePrice * config.BonusMultiplier);
                        totalBonus += vehicleBonus;
                    }
                    else
                    {
                        _logger.LogWarning("Pojazd o ID {VehicleId} nie posiada przypisanej konfiguracji. Zostanie pominięty w wyliczeniach.", vehicle.Id);
                    }
                }

                finalPrice = totalPrice - totalDiscount - totalBonus;

                if (finalPrice < 0)
                {
                    _logger.LogWarning("Wyliczona cena końcowa oferty spadła poniżej zera ({CalculatedPrice} PLN). Cena została zredukowana do 0 PLN.", finalPrice);
                    finalPrice = 0m;
                }

                var result = new List<decimal>
                {
                    totalPrice,
                    totalDiscount,
                    totalBonus,
                    finalPrice
                };

                _logger.LogInformation("Pomyślnie zakończono wyliczanie oferty. Cena bazowa: {TotalPrice}, Bonus: {TotalBonus}, Cena końcowa: {FinalPrice}",
                    totalPrice, totalBonus, finalPrice);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił nieoczekiwany błąd podczas przeliczania cen dla listy {VehicleCount} pojazdów.", vehicles.Count);
                throw;
            }
        }
    }
}
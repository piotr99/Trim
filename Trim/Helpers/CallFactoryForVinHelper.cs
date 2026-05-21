using System;
using System.Threading.Tasks;
using Trim.Models;
using Trim.Models.BussinesCalculactions;

namespace Trim.Helpers
{
    public interface ICallFactoryForVinHelper
    {
        Task<string> GetVinAsync(VehicleConfiguration config);
    }

    public class CallFactoryForVinHelper : ICallFactoryForVinHelper
    {
        private readonly ILogger<OfferCalculator> _logger;
        private static readonly Random _random = new Random();

        private const string AllowedChars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";

        public CallFactoryForVinHelper(ILogger<OfferCalculator> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetVinAsync(VehicleConfiguration config)
        {
            if (config == null)
            {
                _logger.LogWarning("Próba wygenerowania VIN dla pustej konfiguracji (null).");
                throw new ArgumentNullException(nameof(config));
            }

            _logger.LogInformation("Rozpoczęto generowanie VIN dla konfiguracji: {@VehicleConfiguration}", config);

            await Task.Delay(500);

            string wmi = "YS2";
            char[] remainingChars = new char[14];
            for (int i = 0; i < 14; i++)
            {
                remainingChars[i] = AllowedChars[_random.Next(AllowedChars.Length)];
            }

            string generatedVin = wmi + new string(remainingChars);

            _logger.LogInformation("Pomyślnie wygenerowano nowy numer VIN: {GeneratedVin}", generatedVin);

            return generatedVin;
        }
    }
}
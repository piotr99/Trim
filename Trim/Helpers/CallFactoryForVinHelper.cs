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
        private static readonly Random _random = new Random();

        // Pula dozwolonych znaków (bez I, O, Q)
        private const string AllowedChars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";

        // 1. Usunięto modyfikator 'static'
        public async Task<string> GetVinAsync(VehicleConfiguration config)
        {
            await Task.Delay(500);

            string wmi = "YS2";

            char[] remainingChars = new char[14];
            for (int i = 0; i < 14; i++)
            {
                remainingChars[i] = AllowedChars[_random.Next(AllowedChars.Length)];
            }

            return wmi + new string(remainingChars);
        }
    }
}
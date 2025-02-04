using Microsoft.Extensions.Caching.Memory;

namespace TalentAcquisitionModule.Services
{
    public class EmailConfirmationService
    {
        private readonly IMemoryCache _memoryCache;

        public EmailConfirmationService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void StoreConfirmationCode(string userId, string code)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10)); // Code expires after 10 minutes

            _memoryCache.Set(userId, code, cacheEntryOptions);
        }

        public string GenerateRandomCode(int length = 6)
        {
            var random = new Random();
            var code = random.Next(100000, 999999).ToString(); // Generates a 6-digit number
            return code;
        }
    }
}

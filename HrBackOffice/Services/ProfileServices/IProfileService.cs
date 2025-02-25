using Models;

namespace HrBackOffice.Services.ProfileServices
{
    public interface IProfileService
    {
        Task<List<string>> GetAllRolesAsync();
        Task<AppUser> GetUserProfileAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, string displayName, string email);
        Task<bool> UpdateProfileImageAsync(string userId, IFormFile imageFile);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}

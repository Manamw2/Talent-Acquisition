using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;

namespace HrBackOffice.Services.ProfileServices
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProfileService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
        }
        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null).ToListAsync();
        }
        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (result.Succeeded)
                {
                    await _unitOfWork.SaveAsync();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AppUser> GetUserProfileAsync(string userId)
        {
            return await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
            u => u.Id == userId);
        }

        public async Task<bool> UpdateProfileAsync(string userId, string displayName, string email)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                    u => u.Id == userId);

                if (user == null) return false;

                user.DisplayName = displayName;
                user.Email = email;
                user.UserName = email.Split('@')[0];

                var emailChanged = user.Email != email;
                if (emailChanged)
                {
                    // Use UserManager to handle email change as it requires special handling
                    var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, email);
                    var result = await _userManager.ChangeEmailAsync(user, email, emailToken);
                    if (!result.Succeeded) return false;
                }

                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProfileImageAsync(string userId, IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0) return false;

                var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                    u => u.Id == userId);

                if (user == null) return false;

                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ImageUrl.TrimStart('/'));
                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }

                // Save new image
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                user.ImageUrl = $"/images/users/{uniqueFileName}";
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

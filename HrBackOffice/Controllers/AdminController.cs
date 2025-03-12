using HrBackOffice.Models;
using HrBackOffice.Services.ProfileServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace HrBackOffice.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IProfileService _profileService;

        public AdminController(UserManager<AppUser> userManager
                            , SignInManager<AppUser> signInManager
                            , IProfileService profileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _profileService = profileService;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var user = await _userManager.FindByEmailAsync(login.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid email or password");
                return View(login);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View(login);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && !roles.Contains("HR"))
            {
                ModelState.AddModelError(string.Empty, "You are not authorized to access this system");
                return View(login);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            ViewData["UserProfileImage"] = user.ImageUrl;

            return RedirectToAction("Index", "Applicant");
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> ProfileIndex()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var userProfile = await _profileService.GetUserProfileAsync(user.Id);
            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ImageUrl = user.ImageUrl,
                Role = roles.FirstOrDefault()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View("ProfileIndex", model);

            var success = await _profileService.UpdateProfileAsync(
                model.Id,
                model.DisplayName,
                model.Email);

            if (success)
            {
                if (model.ImageFile != null)
                {
                    await _profileService.UpdateProfileImageAsync(model.Id, model.ImageFile);
                }

                TempData["Success"] = "Profile updated successfully";
                return RedirectToAction(nameof(ProfileIndex));
            }

            ModelState.AddModelError("", "Failed to update profile");
            return View("ProfileIndex", model);
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(ProfileIndex));

            var user = await _userManager.GetUserAsync(User);
            var success = await _profileService.ChangePasswordAsync(
                user.Id,
                model.CurrentPassword,
                model.NewPassword);

            if (success)
            {
                TempData["Success"] = "Password changed successfully";
                return RedirectToAction(nameof(ProfileIndex));
            }

            ModelState.AddModelError("", "Failed to change password");
            return RedirectToAction(nameof(ProfileIndex));
        }

    }
}

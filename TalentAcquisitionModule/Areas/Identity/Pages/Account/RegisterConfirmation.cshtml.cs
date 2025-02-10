// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Models;
using TalentAcquisitionModule.Services;

namespace TalentAcquisitionModule.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailSender _emailSender;

        public RegisterConfirmationModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMemoryCache memoryCache, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _memoryCache = memoryCache;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Confirmation Code")]
            public string Code { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string email, bool isFromRegisterPage = true , string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            var storedCode = _memoryCache.Get<string>(user.Id);
            if (storedCode == null || storedCode != Input.Code)
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired confirmation code.");
                return Page();
            }

            // Mark the user's email as confirmed
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            // Clear the confirmation code from the cache
            _memoryCache.Remove(user.Id);

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                if(isFromRegisterPage)
                    return RedirectToPage("ProfileCompletion");
                return RedirectToAction("Index", "Profile");
            }
            else
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
        }

        public async Task<JsonResult> OnPostResendCodeAsync([FromBody] ResendCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return new JsonResult(new { success = false, message = "Email is required." });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found." });
            }

            // Generate a new confirmation code
            var confirmationCode = new Random().Next(100000, 999999).ToString();
            _memoryCache.Set(user.Id, confirmationCode, TimeSpan.FromMinutes(15));

            // Send confirmation code via email
            await _emailSender.SendEmailAsync(request.Email, "Confirm your email",
                        $"Your confirmation code is: <strong>{confirmationCode}</strong>. Please enter this code on the confirmation page to verify your account.");

            return new JsonResult(new { success = true });
        }

        public class ResendCodeRequest
        {
            public string Email { get; set; }
        }
    }
}


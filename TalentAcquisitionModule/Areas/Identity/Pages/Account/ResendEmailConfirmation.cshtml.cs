// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
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
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;
        public ResendEmailConfirmationModel(UserManager<AppUser> userManager, IEmailSender emailSender, IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "This email address is not registerd");
                return Page();
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "This email is already confirmed.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            EmailConfirmationService emailService = new EmailConfirmationService(_memoryCache);
            var code = emailService.GenerateRandomCode(); // Generate a random 6-digit code
            emailService.StoreConfirmationCode(userId, code); // Store the code in memory cache
            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Your confirmation code is: <strong>{code}</strong>. Please enter this code on the confirmation page to verify your account.");
            return RedirectToPage("RegisterConfirmation", new { email = Input.Email});
        }
    }
}

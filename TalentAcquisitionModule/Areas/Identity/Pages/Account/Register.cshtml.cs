// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;
using Models.Mappers;
using Models.ViewModels;
using Newtonsoft.Json;
using TalentAcquisitionModule.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TalentAcquisitionModule.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            IMemoryCache memoryCache,
            HttpClient httpClient,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
            _httpClient = httpClient;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Display(Name = "Email Address")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DisplayName("Name")]
            public string Name { get; set; } = string.Empty;

            [Required]
            [DisplayName("University")]
            public string University { get; set; } = string.Empty;
            public IEnumerable<SelectListItem> Universities { get; set; }


            [Required]
            [DisplayName("Faculty")]
            public string Faculty { get; set; } = string.Empty;
            public IEnumerable<SelectListItem> Faculties { get; set; }


            [Required]
            [Phone]
            [DisplayName("Phone Number")]
            public string Phone { get; set; }

            [Required]
            [DisplayName("Date of Birth")]
            public DateOnly DateOfBirth { get; set; }

            [Required]
            [DisplayName("Education Level")]
            public string EducationLevel { get; set; }
            public IEnumerable<SelectListItem> EducationLevels { get; set; }

            [Required]
            [DisplayName("English Proficiency Level")]
            public string EnglishProficiencyLevel { get; set; }
            public IEnumerable<SelectListItem> EnglishProficiencyLevels { get; set; }

            [Required]
            [DisplayName("Method of Contact")]
            public string MethodOfContact { get; set; }
            public IEnumerable<SelectListItem> MethodOfContactOptions { get; set; }

            [ValidateNever]
            public string CvUrl { get; set; }


            /*[Required]
            [DisplayName("Education Level")]
            public string EducationLevel { get; set; }

            [Required]
            [DisplayName("English Proficiency Level")]
            public string EnglishProficiencyLevel { get; set; }*/


        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            InitDropDowns();
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        [BindProperty]
        public IFormFile CvFile { get; set; } // Add this property for file upload

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Handle CV file upload
                if (CvFile != null && CvFile.Length > 0)
                {
                    try
                    {
                        var fileName = Path.GetRandomFileName() + Path.GetExtension(CvFile.FileName); // Safer filename
                        string sharedFolderPath = FileStorageService.GetSharedCsvFolderPath();
                        string filePath = Path.Combine(sharedFolderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await CvFile.CopyToAsync(stream);
                        }
                        user.CvUrl = filePath;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, $"CV upload failed: {ex.Message}");
                        InitDropDowns();
                        return Page();
                    }
                }

                user.DisplayName = Input.Name;
                user.BirthDate = Input.DateOfBirth;
                user.University = Input.University;
                user.Faculty = Input.Faculty;
                user.PhoneNumber = Input.Phone;
                user.EducationLevel = Input.EducationLevel;
                user.EnglishLevel = Input.EnglishProficiencyLevel;
                user.MethodOfContact = Input.MethodOfContact;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    result = await _userManager.AddToRoleAsync(user, "applicant");

                    var userId = await _userManager.GetUserIdAsync(user);
                    EmailConfirmationService emailService = new EmailConfirmationService(_memoryCache);
                    var code = emailService.GenerateRandomCode(); // Generate a random 6-digit code
                    emailService.StoreConfirmationCode(userId, code); // Store the code in memory cache

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Your confirmation code is: <strong>{code}</strong>. Please enter this code on the confirmation page to verify your account.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    InitDropDowns();
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            InitDropDowns();
            return Page();
        }


        public async Task<IActionResult> OnPostUploadCvAsync()
        {
            if (CvFile == null || CvFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload a CV file.");
                InitDropDowns();
                return Page();
            }

            try
            {
                // Call your API to extract data from the CV
                var profileInfoVM = await ExtractDataFromCv(CvFile);
                var fileName = Path.GetRandomFileName() + Path.GetExtension(CvFile.FileName); // Safer filename
                string sharedFolderPath = FileStorageService.GetSharedCsvFolderPath();
                string filePath = Path.Combine(sharedFolderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CvFile.CopyToAsync(stream);
                }
                profileInfoVM.CvUrl = filePath;
                TempData["ProfileInfo"] = JsonSerializer.Serialize(profileInfoVM);
                return RedirectToAction("ConfirmResumeInfo", "Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"CV data extraction failed: {ex.Message}");
            }

            InitDropDowns();
            return Page();
        }

        private async Task<ProfileInfoVM> ExtractDataFromCv(IFormFile cvFile)
        {
            var content = new MultipartFormDataContent();
            // Read the file content from cvFile into a MemoryStream
            using (var memoryStream = new MemoryStream())
            {
                await cvFile.CopyToAsync(memoryStream); // Copy the file content to the MemoryStream
                memoryStream.Position = 0; // Reset the stream position to the beginning

                // Create ByteArrayContent from the MemoryStream
                var fileContentStream = new ByteArrayContent(memoryStream.ToArray());

                // Set the Content-Disposition header
                fileContentStream.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "file", // Name of the form field
                    FileName = cvFile.FileName // Name of the file
                };

                // Set the Content-Type header based on the file type
                fileContentStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(cvFile.ContentType);

                // Add the file content to the MultipartFormDataContent
                content.Add(fileContentStream);

                // Make the API call
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/parse-resume", content);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                ResumeViewModel resumeViewModel = JsonSerializer.Deserialize<ResumeViewModel>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var profileVM = resumeViewModel.ResumeToProfileInfo();
                return profileVM;
            }
        }


        private void InitDropDowns()
        {
            IEnumerable<string> Universities = [
                 "Cairo University",
                "Ain Shams University",
                "Alexandria University",
                "Helwan University",
                "Mansoura University",
                "Zagazig University",
                "Assiut University",
                "Tanta University",
                "Benha University",
                "Suez Canal University",
                "Minia University",
                "South Valley University",
                "Fayoum University",
                "Beni-Suef University",
                "Sohag University",
                "Kafr El Sheikh University",
                "Damietta University",
                "Port Said University",
                "Menoufia University",
                "Al-Azhar University",
                "The British University in Egypt (BUE)",
                "The American University in Cairo (AUC)",
                "German University in Cairo (GUC)",
                "Misr University for Science and Technology (MUST)",
                "Future University in Egypt (FUE)",
                "October 6 University",
                "Modern Sciences and Arts University (MSA)",
                "Nahda University",
                "Sinai University"
                ];
           IEnumerable<string> Faculties = [ "Faculty of Engineering",
                "Faculty of Medicine",
                "Faculty of Pharmacy",
                "Faculty of Science",
                "Faculty of Commerce",
                "Faculty of Law",
                "Faculty of Arts",
                "Faculty of Education",
                "Faculty of Agriculture",
                "Faculty of Dentistry",
                "Faculty of Computer and Artificial Intelligence",
                "Faculty of Veterinary Medicine",
                "Faculty of Nursing",
                "Faculty of Physical Therapy",
                "Faculty of Tourism and Hotels",
                "Faculty of Mass Communication",
                "Faculty of Fine Arts",
                "Faculty of Applied Arts",
                "Faculty of Al-Alsun (Languages)",
                "Faculty of Islamic Studies"
            ];
            IEnumerable<string> EducationLevels = ["Undergraduate", "Graduate"];
            IEnumerable<string> EnglishProficiencyLevels = ["Beginner", "Intermediate", "Advanced", "Fluent"];
            IEnumerable<string> MethodOfContactOptions = ["Email", "Phone"];
            Input = new()
            {
                Universities = Universities.Select(u => new SelectListItem
                {
                    Text = u,
                    Value = u
                }),

                Faculties = Faculties.Select(u => new SelectListItem
                {
                    Text = u,
                    Value = u
                }),
                EducationLevels = EducationLevels.Select(u => new SelectListItem
                {
                    Text = u,
                    Value = u
                }),
                EnglishProficiencyLevels = EnglishProficiencyLevels.Select(u => new SelectListItem
                {
                    Text = u,
                    Value = u
                }),
                MethodOfContactOptions = MethodOfContactOptions.Select(u => new SelectListItem
                {
                    Text = u,
                    Value = u
                }),
            };
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}

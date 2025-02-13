using AutoMapper;
using DataAccess.Repository.IRepository;
using HrBackOffice.Helper.ApplicantService;
using HrBackOffice.Helper.EmailSetting;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Mappers;
using Models.ViewModels;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using X.PagedList.Extensions;

namespace HrBackOffice.Controllers
{
    public class ApplicantController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSend _emailSender;
        private readonly HttpClient _httpClient;
        private readonly IApplicantService _AppSevice;

        public ApplicantController(IApplicantService applicantService,HttpClient httpClient,IEmailSend emailSender,IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClient;
            _AppSevice= applicantService;
        }
      
        public async Task<IActionResult> Index(int? page, string searchQuery = null)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;
            var applicants = new List<UserViewModel>();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"http://localhost:8000/search?query={Uri.EscapeDataString(searchQuery)}&max_results=5&exact_thresh=0.9&nonexact_thresh=0.5");

                    if (response.IsSuccessStatusCode)
                    {
                        var searchResult = await response.Content.ReadFromJsonAsync<SearchResult>();
                        var matchedUserIds = searchResult.Results.Select(r => r.Id).ToList();

                        // Get only the users that match the IDs from the search results
                        foreach (var userId in matchedUserIds)
                        {
                            var user = await _userManager.FindByIdAsync(userId);
                            if (user != null && await _userManager.IsInRoleAsync(user, "Applicant"))
                            {
                                applicants.Add(new UserViewModel
                                {
                                    Id = user.Id,
                                    UserName = user.UserName,
                                    Email = user.Email,
                                    EducationLevel = user.EducationLevel,
                                    EnglishProficiencyLevel = user.EnglishLevel,
                                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                // Default behavior when no search query
                var users = await _userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, "Applicant"))
                    {
                        applicants.Add(new UserViewModel
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            EducationLevel = user.EducationLevel,
                            EnglishProficiencyLevel = user.EnglishLevel,
                            Roles = (await _userManager.GetRolesAsync(user)).ToList()
                        });
                    }
                }
            }

            var pagedApplicant = applicants.ToPagedList(pageNumber, pageSize);
            return View(pagedApplicant);
        }
        

        public IActionResult AddApplicant()
        {
            var model = new UserViewModel
            {
                ApplicantExperiences = new List<AppExperienceViewModel>(), // Ensure list is not null
                ApplicantSkills = new List<AppSkillViewModel>(),
                ApplicantProjects = new List<AppProjectViewModel>()
            };
            _AppSevice.PopulateDropdownLists(model);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddApplicant(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine(string.Join("\n", errors)); // Check logs for the actual errors
                _AppSevice.PopulateDropdownLists(model);
                return View(model);
            }

            var university = model.University == "Other" ? Request.Form["University"].ToString() : model.University;
            var faculty = model.Faculty == "Other" ? Request.Form["Faculty"].ToString() : model.Faculty;

            var user = new AppUser
            {
                UserName = model.UserName,
                DisplayName = model.DisplayName,
                Email = model.Email,
                PhoneNumber = model.Phone,
                EnglishLevel = model.EnglishProficiencyLevel,
                EducationLevel = model.EducationLevel,
                Faculty = model.Faculty,
                MethodOfContact = model.MethodOfContact,
                BirthDate = model.BirthDate,
                CvUrl = model.CvUrl
            };
            
            var result = await _userManager.CreateAsync(user, "Temp@1234"); // Default password
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            foreach (var experience in model.ApplicantExperiences)
            {
                var newExperience = new ApplicantExperience
                {
                    Company = experience.Company,
                    Position = experience.Position,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    Description = experience.Description,
                    AppUserId = user.Id
                };
                await _unitOfWork.AppExpRepository.AddAsync(newExperience);
            }

            // Add Skills
            foreach (var skill in model.ApplicantSkills)
            {
                var newSkill = new ApplicantSkill
                {
                    Name = skill.Name,
                    Level = skill.Level,
                    AppUserId = user.Id
                };
                await _unitOfWork.AppSkillRepository.AddAsync(newSkill);
            }

            // Add Projects
            foreach (var project in model.ApplicantProjects)
            {
                var newProject = new ApplicantProject
                {
                    Name = project.Name,
                    Description = project.Description,
                    AppUserId = user.Id
                };
                await _unitOfWork.AppProjRepository.AddAsync(newProject);
            }

            await _unitOfWork.SaveAsync();
            await _userManager.AddToRoleAsync(user, "Applicant");


            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            string careerBaseUrl = "https://localhost:7142";
            

            var resetLink = $"{careerBaseUrl}/Identity/Account/ResetPassword?code={Uri.EscapeDataString(code)}";

            string emailMessage = $"Welcome {model.DisplayName}," +
                $" <p>Your account has been created on our careers platform.</p>" +
                $"    </ul> <br/> Click <a href='{HtmlEncoder.Default.Encode(resetLink)}'>{HtmlEncoder.Default.Encode(resetLink)}</a> to reset your password.";
      
            await _emailSender.SendEmailAsync(model.Email, "Welcome to Our Career Portal", emailMessage);

            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public async Task<IActionResult> ExtractDataFromCv(UserViewModel model)
        {
            if (model.CvFile == null || model.CvFile.Length == 0)
            {
                ModelState.AddModelError("CvFile", "Please upload a valid CV.");
                return View("AddApplicant", model);
            }
            _AppSevice.PopulateDropdownLists(model);

            // Call API to extract data
            var extractedData = await _AppSevice.ExtractDataFromCv(model.CvFile);

            // Populate model with extracted data
            model.UserName = extractedData.UserName;
            model.DisplayName = extractedData.DisplayName;
            model.Email = extractedData.Email;
            model.Phone = extractedData.Phone;
            model.EducationLevel = extractedData.EducationLevel;
            model.EnglishProficiencyLevel = extractedData.EnglishProficiencyLevel;
            model.Faculty = extractedData.Faculty;
            model.BirthDate = extractedData.BirthDate;
            model.MethodOfContact = extractedData.MethodOfContact;
            model.ApplicantExperiences = extractedData.ApplicantExperiences;
            model.ApplicantSkills = extractedData.ApplicantSkills;
            model.ApplicantProjects = extractedData.ApplicantProjects;

            return View("AddApplicant", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApplicant(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid applicant ID.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Applicant not found.");
            }

            // Delete related data
            var experiences = await _unitOfWork.AppExpRepository.GetAllAsync(e => e.AppUserId == id);
            var skills = await _unitOfWork.AppSkillRepository.GetAllAsync(s => s.AppUserId == id);
            var projects = await _unitOfWork.AppProjRepository.GetAllAsync(p => p.AppUserId == id);

            _unitOfWork.AppExpRepository.RemoveRange(experiences);
            _unitOfWork.AppSkillRepository.RemoveRange(skills);
            _unitOfWork.AppProjRepository.RemoveRange(projects);

            await _unitOfWork.SaveAsync();

            // Remove user from database
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Error deleting the applicant.");
            }

            return Ok(new { success = true, message = "Applicant deleted successfully!" });
        }



    }
}

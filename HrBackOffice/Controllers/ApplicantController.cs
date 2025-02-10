using AutoMapper;
using DataAccess.Repository.IRepository;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using X.PagedList.Extensions;

namespace HrBackOffice.Controllers
{
    public class ApplicantController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        public ApplicantController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #region comm
        //public async Task<IActionResult> Index(int? page)
        //{
        //    int pageSize = 5; // Number of items per page
        //    int pageNumber = page ?? 1; // Default to page 1 if no page is specified
        //    var users = await _userManager.Users.ToListAsync();
        //    var applicants = new List<UserViewModel>();

        //    foreach (var user in users)
        //    {
        //        if (await _userManager.IsInRoleAsync(user, "Applicant"))
        //        {
        //            applicants.Add(new UserViewModel
        //            {
        //                Id = user.Id,
        //                UserName = user.UserName,
        //                Email = user.Email,
        //                EducationLevel = user.EducationLevel,
        //                EnglishLevel = user.EnglishLevel,
        //                Roles = (await _userManager.GetRolesAsync(user)).ToList()
        //            });
        //        }
        //    }


        //    var pagedApplicant = applicants.ToPagedList(pageNumber, pageSize);
        //    return View(pagedApplicant);
        //}
        #endregion

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
                                    EnglishLevel = user.EnglishLevel,
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
                            EnglishLevel = user.EnglishLevel,
                            Roles = (await _userManager.GetRolesAsync(user)).ToList()
                        });
                    }
                }
            }

            var pagedApplicant = applicants.ToPagedList(pageNumber, pageSize);
            return View(pagedApplicant);
        }





    }
}

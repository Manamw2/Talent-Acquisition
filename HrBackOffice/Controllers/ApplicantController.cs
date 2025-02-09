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

        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 5; // Number of items per page
            int pageNumber = page ?? 1; // Default to page 1 if no page is specified
            var users = await _userManager.Users.ToListAsync();
            var applicants = new List<UserViewModel>();

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

            
            var pagedApplicant = applicants.ToPagedList(pageNumber, pageSize);
            return View(pagedApplicant);
        }




    }
}

using AutoMapper;
using DataAccess.Repository.IRepository;
using Models;
using Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TalentAcquisitionModule.Controllers
{
    [Route("TalentAcquisition/Applicants")]
    public class ApplicantController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ApplicantController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var jobs = await _unitOfWork.JobRepository.GetAllWithDetailsAsync() ?? new List<Job>();
            var jobDTOs = _mapper.Map<IEnumerable<JobViewModel>>(jobs);
            //ViewBag.Departments = _unitOfWork.DepartmentRepository.GetAllAsync();
            //ViewBag.Batchs = _unitOfWork.BatchRepository.GetAllAsync();
            return View(jobDTOs);
        }

        [Route("FirstView")]
        public IActionResult FirstView()
        {
            return View();
        }

        [HttpGet]
        [Route("ApplyForJob")]
        public async Task<IActionResult> ApplyForJob(int jobId)
        {
            try
            {
                var applicantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(applicantId))
                {
                    return Json(new { success = false, message = "Applicant not found. Please log in." });
                }

                var jobApplication = new JobApplication
                {
                    UserId = applicantId,
                    JobId = jobId,
                    AppliedDate = DateTime.UtcNow,
                };

                await _unitOfWork.JobApplicationRepository.AddAsync(jobApplication);
                await _unitOfWork.SaveAsync(); // Ensure changes are saved

                return Json(new { success = true, message = "You applied successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Log the error message
                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }
    }
}

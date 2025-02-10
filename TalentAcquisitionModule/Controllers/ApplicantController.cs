using AutoMapper;
using DataAccess.Repository.IRepository;
using Models;
using Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

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

        //public async Task<IActionResult> Index()  // for apply jobs to applicants
        //{
        //    var jobs = await _unitOfWork.JobRepository.GetAllWithDetailsAsync() ?? new List<Job>();
        //    var jobDTOs = _mapper.Map<IEnumerable<JobViewModel>>(jobs);
        //    return View(jobDTOs);
        //}
        [Route("Applicant/{page:int?}")]
        public async Task<IActionResult> Index(int page = 1)  // Add a default page parameter
        {
            const int pageSize = 5; // Number of jobs per page

            // Fetch all jobs
            var jobs = await _unitOfWork.JobRepository.GetAllWithDetailsAsync() ?? new List<Job>();

            foreach (var job in jobs)
            {
                Console.WriteLine($"Job ID: {job.JobId}, Batch ID: {job.BatchId}, Batch EndDate: {job.Batch?.EndDate}");
            }


            var jobDTOs = _mapper.Map<IEnumerable<JobViewModel>>(jobs);

     

            // Paginate the jobs
            var paginatedJobs = jobDTOs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Calculate total pages
            var totalJobs = jobDTOs.Count();
            var totalPages = (int)Math.Ceiling(totalJobs / (double)pageSize);

            // Pass data to the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedJobs);
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
                await _unitOfWork.SaveAsync(); 

                return Json(new { success = true, message = "You applied successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); 
                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }
    }
}

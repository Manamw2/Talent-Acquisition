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

        

        [Route("Applicant/{page:int?}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 5; // Number of jobs per page

            // Fetch all jobs
            var jobs = await _unitOfWork.JobRepository.GetAllWithDetailsAsync() ?? new List<Job>();

            // Filter for active batches (where batch end date is after today)
            var today = DateTime.Today;
            jobs = jobs.Where(job => job.Batch != null && job.Batch.EndDate > today).ToList();

            foreach (var job in jobs)
            {
                Console.WriteLine($"Job ID: {job.JobId}, Batch ID: {job.BatchId}, Batch EndDate: {job.Batch?.EndDate}");
            }

            var jobDTOs = jobs.Select(job => new JobViewModel
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description,
                JobType = job.JobType,
                DepartmentId = job.DepartmentId,
                DepartmentName = job.Department.Name,
                BatchId = job.BatchId,
                EndDate = job.Batch.EndDate
            }).ToList();

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

                // Fetch the job details
                var job = await _unitOfWork.JobRepository.GetFirstOrDefaultAsync(x => x.JobId == jobId);
                if (job == null)
                {
                    return Json(new { success = false, message = "Job not found." });
                }

                // Check if the job type is FullTime
                if (job.JobType == JobType.FullTime)
                {
                    // Check if user has completed all profile sections
                    var hasExperience = await _unitOfWork.AppExpRepository.GetFirstOrDefaultAsync(x => x.AppUserId == applicantId) != null;
                    var hasSkills = await _unitOfWork.AppSkillRepository.GetFirstOrDefaultAsync(x => x.AppUserId == applicantId) != null;
                    var hasProjects = await _unitOfWork.AppProjRepository.GetFirstOrDefaultAsync(x => x.AppUserId == applicantId) != null;

                    var incompleteProfileMessage = GetIncompleteProfileMessage(hasExperience, hasSkills, hasProjects);
                    if (!string.IsNullOrEmpty(incompleteProfileMessage))
                    {
                        return Json(new
                        {
                            success = false,
                            message = incompleteProfileMessage,
                            requiresProfileCompletion = true,
                            missingData = new
                            {
                                experience = !hasExperience,
                                skills = !hasSkills,
                                projects = !hasProjects
                            }
                        });
                    }
                }

                // Check if the user has already applied for this job
                var application = await _unitOfWork.JobApplicationRepository.GetFirstOrDefaultAsync(x => x.UserId == applicantId && jobId == x.JobId);
                if (application != null)
                {
                    return Json(new { success = false, message = "You have already applied to this job" });
                }

                // Create a new job application
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

        private string GetIncompleteProfileMessage(bool hasExperience, bool hasSkills, bool hasProjects)
        {
            var missingItems = new List<string>();

            if (!hasExperience) missingItems.Add("experience");
            if (!hasSkills) missingItems.Add("skills");
            if (!hasProjects) missingItems.Add("projects");

            if (missingItems.Count == 0) return string.Empty;

            if (missingItems.Count == 1)
                return $"Please add your {missingItems[0]} before applying for jobs.";

            if (missingItems.Count == 2)
                return $"Please add your {missingItems[0]} and {missingItems[1]} before applying for jobs.";

            return "Please complete your profile (experience, skills, and projects) before applying for jobs.";
        }
    }
}

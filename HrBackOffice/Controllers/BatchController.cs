using AutoMapper;
using DataAccess.Repository.IRepository;
using Hangfire.Common;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using Models;



namespace HrBackOffice.Controllers
{
    [Authorize]
    public class BatchController : Controller
	{
        private readonly ILogger<ApplicantController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public BatchController(ILogger<ApplicantController> logger,IConfiguration configuration,
            UserManager<AppUser> userManager,IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _config = configuration;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5;

            // Get total count for pagination
            var totalBatches = await _unitOfWork.BatchRepository.CountAsync();

            // Get only the batches for the current page
            var batches = await _unitOfWork.BatchRepository.GetPagedListAsync(
                includeProperties: "Job",
                pageIndex: page - 1,
                pageSize: pageSize
            );
            var viewModels = batches.Select(b => new BatchViewModel
            {
                Id = b.BatchId,
                BatchName = b.BatchName,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                JobId = b.JobId,
                JobTitle = b.Job?.Title,
                Status = b.Status,
                TargetNumber = b.TargetNumber
            }).ToList();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalBatches / (double)pageSize);

            // Pass data to the view
            ViewBag.TotalItems = totalBatches;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(viewModels);
        }

        public async Task<IActionResult> Create()
        {
            var batch = new BatchViewModel
            {
                StartDate = DateTime.Now, // Set default StartDate
                Status = BatchStatus.New, // Set default status
                TargetNumber = 1 // Set default target number
            };

            // Populate jobs dropdown
            await PopulateJobsDropdownAsync();

            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BatchViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                // Map ViewModel to domain model
                var batch = _mapper.Map<Batch>(model);
                _unitOfWork.BatchRepository.AddAsync(batch);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate jobs dropdown on validation error
            await PopulateJobsDropdownAsync();

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var batch = await _unitOfWork.BatchRepository.GetFirstOrDefaultAsync(b => b.BatchId == id);
            if (batch == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<BatchViewModel>(batch);
            model.Id = batch.BatchId;
            // Populate jobs dropdown
            await PopulateJobsDropdownAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BatchViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var batch = _mapper.Map<Batch>(model);
                batch.BatchId = id;
                _unitOfWork.BatchRepository.Update(batch);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));

               
            }

            // Repopulate jobs dropdown on validation error
            await PopulateJobsDropdownAsync();

            return View(model);
        }

        // Helper method to populate jobs dropdown
        private async Task PopulateJobsDropdownAsync()
        {
            // Get all jobs from the database
            var jobs = await _unitOfWork.JobRepository.GetAllAsync();

            // Create SelectList for dropdown
            ViewBag.Jobs = new SelectList(jobs, "JobId", "Title");
        }


        public async Task<IActionResult> Delete(int id)
        {
            var batch = await _unitOfWork.BatchRepository
                .GetFirstOrDefaultAsync(filter: b => b.BatchId == id, includeProperties: "Job");
            if (batch == null)
            {
                return NotFound();
            }

            // Check if any jobs are assigned to this batch
            var isBatchAssigned = await _unitOfWork.JobRepository
                .GetFirstOrDefaultAsync(j => j.BatchId == id) != null;

            // Only allow deletion if NO jobs are assigned (opposite of current logic)
            if (!isBatchAssigned)
            {
                _unitOfWork.BatchRepository.Remove(batch);
                await _unitOfWork.SaveAsync();
                TempData["Success"] = "Batch deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Cannot delete batch because it is assigned to a job.";
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> GetApplicant(int id, string searchQuery = null)
        {
            // Validate batch ID
            if (id <= 0)
            {
                return BadRequest("Invalid batch ID");
            }

            try
            {
                var batch = await _unitOfWork.BatchRepository.GetFirstOrDefaultAsync(
                    filter: B => B.BatchId == id,
                    includeProperties: "JobApplications.AppUser"
                );

                if (batch == null)
                {
                    return NotFound();
                }

                var applications = new List<JobApplicationVM>();

                // Only perform external search if query is provided
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    using (var client = new HttpClient())
                    {
                        var link = _config["Search"];

                        // Construct the search URL with proper encoding
                        var searchUrl = $"{link}query={Uri.EscapeDataString(searchQuery)}&max_results=5&exact_thresh=0.9&nonexact_thresh=0.5";

                        try
                        {
                            var response = await client.GetAsync(searchUrl);

                            // Ensure success status code
                            response.EnsureSuccessStatusCode();

                            var searchResult = await response.Content.ReadFromJsonAsync<SearchResult>();

                            if (searchResult?.Results != null)
                            {
                                var matchedUserIds = searchResult.Results.Select(r => r.Id).ToList();

                                // Filter applications based on matched user IDs
                                applications = batch.JobApplications
                                    .Where(app => matchedUserIds.Contains(app.UserId))
                                    .Select(app => new JobApplicationVM
                                    {
                                        ApplicationId = app.ApplicationId,
                                        UserId = app.UserId,
                                        ApplicantName = app.AppUser.DisplayName ?? app.AppUser.UserName,
                                        ApplicantEmail = app.AppUser.Email,
                                        AppliedDate = app.AppliedDate,
                                        Status = app.Status
                                    }).ToList();
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            // Log the error but continue with full list
                            _logger.LogError(ex, "External search API request failed");

                            // Fallback to full list if API fails
                            applications = batch.JobApplications
                                .Select(app => new JobApplicationVM
                                {
                                    ApplicationId = app.ApplicationId,
                                    UserId = app.UserId,
                                    ApplicantName = app.AppUser.DisplayName ?? app.AppUser.UserName,
                                    ApplicantEmail = app.AppUser.Email,
                                    AppliedDate = app.AppliedDate,
                                    Status = app.Status
                                }).ToList();
                        }
                    }
                }
                else
                {
                    // If no search query, return all applications
                    applications = batch.JobApplications
                        .Select(app => new JobApplicationVM
                        {
                            ApplicationId = app.ApplicationId,
                            UserId = app.UserId,
                            ApplicantName = app.AppUser.DisplayName ?? app.AppUser.UserName,
                            ApplicantEmail = app.AppUser.Email,
                            AppliedDate = app.AppliedDate,
                            Status = app.Status
                        }).ToList();
                }

                return View(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applicants for batch {BatchId}", id);
                return StatusCode(500, "An error occurred while retrieving applicants");
            }
        }
        public IActionResult ApplicantProfile(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var applicant = _userManager.Users
                .Include(u => u.ApplicantExperiences)
                .Include(u => u.ApplicantSkills)
                .Include(u => u.ApplicantProjects)
                .FirstOrDefault(u => u.Id == userId);

            if (applicant == null)
                return NotFound();

            return View(applicant);
        }

        [HttpGet]
        public IActionResult GetCv(string filePath, string fileName, bool download = false)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/pdf";

            // Set Content-Disposition: "inline" to display, "attachment" to force download
            var contentDisposition = download ? "attachment" : "inline";

            Response.Headers["Content-Disposition"] = $"{contentDisposition}; filename=\"{fileName}\"";

            return File(fileBytes, contentType);
        }

    }
}

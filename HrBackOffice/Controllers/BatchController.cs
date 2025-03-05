using AutoMapper;
using DataAccess.Repository.IRepository;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using System.Drawing.Printing;
using JobApplicationVM = HrBackOffice.Models.JobApplicationVM;


namespace HrBackOffice.Controllers
{
    [Authorize]
    public class BatchController : Controller
	{
        private readonly ILogger<ApplicantController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        public BatchController(ILogger<ApplicantController> logger,IConfiguration configuration,UserManager<AppUser> userManager,IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _config = configuration;
            _userManager = userManager;
            _logger = logger;

        }
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5;

            // Get total count for pagination
            var totalBatches = await _unitOfWork.BatchRepository.CountAsync();

            // Get only the batches for the current page
            var batches = await _unitOfWork.BatchRepository.GetPagedListAsync(
                pageIndex: page - 1,
                pageSize: pageSize
            );

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalBatches / (double)pageSize);

            // Pass data to the view
            ViewBag.TotalItems = totalBatches;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(batches);
        }

        public IActionResult Create()
        {
            var batch = new Batch
            {
                StartDate = DateTime.Now // Set default StartDate
            };
            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Batch batch)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.BatchRepository.AddAsync(batch);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(batch);
        }
        [HttpPost]
        public async Task<IActionResult> CreateBatch(Batch model)
        {
            if (ModelState.IsValid)
            {
                // Save batch to database (example using EF Core)
                await _unitOfWork.BatchRepository.AddAsync(model);
                await _unitOfWork.SaveAsync();
                TempData["NewBatchId"] = model.BatchId;
                return RedirectToAction("Create", "Job");
            }

            return PartialView("_CreateBatchPartial", model);
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

            if (isBatchAssigned)
            {
                TempData["Error"] = "Cannot delete batch because it is assigned to a job.";
                return RedirectToAction("Index");
            }

            _unitOfWork.BatchRepository.Remove(batch);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = "Batch deleted successfully.";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id)
        {
            var batch = await _unitOfWork.BatchRepository.GetFirstOrDefaultAsync(b => b.BatchId == id);
            if (batch == null)
            {
                return NotFound();
            }
            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Batch batch)
        {
            if (id != batch.BatchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.BatchRepository.Update(batch);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(batch);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] BatchViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create batch entity from model
                    var batch = new Batch
                    {
                        BatchName = model.Name,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate
                        // Add any other necessary properties
                    };

                    // Add to database
                    _unitOfWork.BatchRepository.AddAsync(batch);
                    await _unitOfWork.SaveAsync();

                    // Return success with the new batch ID
                    return Json(new { success = true, batchId = batch.BatchId });
                }
                catch (Exception ex)
                {
                    // Log the exception
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            // If model state is invalid, return validation errors
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, message = string.Join(", ", errors) });
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

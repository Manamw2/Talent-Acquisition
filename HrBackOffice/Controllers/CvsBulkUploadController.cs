using DataAccess.Repository.IRepository;
using Hangfire;
using Hangfire.Storage;
using HrBackOffice.Helper.FileProcessingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Security.Claims;

namespace HrBackOffice.Controllers
{
    [Authorize(Roles = "hr, Admin")]
    public class CvsBulkUploadController : Controller
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly PdfProcessingJob _processingJob;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public CvsBulkUploadController(IBackgroundJobClient backgroundJobClient, PdfProcessingJob pdfProcessingJob, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _backgroundJobClient = backgroundJobClient;
            _processingJob = pdfProcessingJob;
            _webHostEnvironment = webHostEnvironment;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.IsJobRunning = await IsJobRunningAsync();
            IEnumerable<BulkCvsJobsHistory> jobs = await _unitOfWork.BulkCvsJobsHistoryRepo.GetAllAsync();
            return View(jobs);
        }

        public async Task<IActionResult> UploadingFiles()
        {
            ViewBag.IsJobRunning = await IsJobRunningAsync();
            return View();
        }

        public IActionResult Results()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var jobs = await _unitOfWork.BulkCvsJobsHistoryRepo.GetAllAsync();
            return Json(new { data = jobs });
        }

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            try
            {
                if (await IsJobRunningAsync())
                {
                    return Json(new
                    {
                        success = false,
                        message = "A processing job is already running. Please wait until it finishes."
                    });
                }

                string jobId = Guid.NewGuid().ToString("N");
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                AppUser appUser = await _userManager.FindByIdAsync(userId);

                BulkCvsJobsHistory jobHistory = new BulkCvsJobsHistory
                {
                    JobId = jobId,
                    StartDate = DateTime.Now,
                    IsRunning = true,
                    TotalFiles = files.Count,
                    StartedBy = appUser.DisplayName ?? appUser.Email,
                };

                await _unitOfWork.BulkCvsJobsHistoryRepo.AddAsync(jobHistory);
                await _unitOfWork.SaveAsync();


                List<string> filePaths = new List<string>();

                // Save files and store paths
                foreach (var file in files)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "cvs");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    filePaths.Add(filePath);
                }

                // Enqueue background job with file paths
                _backgroundJobClient.Enqueue(() => _processingJob.ProcessFiles(filePaths, jobId));

                return Json(new
                {
                    success = true,
                    jobId = jobId,
                    message = "Files uploaded and processing started"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error starting processing: {ex.Message}"
                });
            }
        }

        // Method to check if any job is currently running
        public async Task<bool> IsJobRunningAsync()
        {
            // Check the database to see if any job is marked as running
            var runningJob = await _unitOfWork.BulkCvsJobsHistoryRepo
                .GetFirstOrDefaultAsync(j => j.IsRunning);

            return runningJob != null;
        }

    }
}

using Hangfire;
using Hangfire.Storage;
using HrBackOffice.Helper.FileProcessingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace HrBackOffice.Controllers
{
    [Authorize(Roles = "hr, Admin")]
    public class CvsBulkUploadController : Controller
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly PdfProcessingJob _processingJob;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CvsBulkUploadController(IBackgroundJobClient backgroundJobClient, PdfProcessingJob pdfProcessingJob, IWebHostEnvironment webHostEnvironment)
        {
            _backgroundJobClient = backgroundJobClient;
            _processingJob = pdfProcessingJob;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var processingJobs = monitoringApi.ProcessingJobs(0, 10); // Gets up to 10 active jobs

            ViewBag.IsJobRunning = processingJobs.Any();

            return View();
        }

        [HttpPost]
        public IActionResult Upload(List<IFormFile> files)
        {
            try
            {
                // ✅ Check if a job is already running
                var monitoringApi = JobStorage.Current.GetMonitoringApi();
                var processingJobs = monitoringApi.ProcessingJobs(0, 10); // Gets up to 10 active jobs
                if (processingJobs.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "A processing job is already running. Please wait until it finishes."
                    });
                }

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
                var jobId = _backgroundJobClient.Enqueue(() =>
                    _processingJob.ProcessFiles(filePaths, null));

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

    }
}

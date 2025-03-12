using AutoMapper;
using DataAccess.Repository.IRepository;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq.Expressions;
using X.PagedList; // Add this
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;

namespace HrBackOffice.Controllers
{
    [Authorize]
    public class JobController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        public JobController(IConfiguration configuration,IUnitOfWork unitOfWork, IMapper mapper , UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _config = configuration;
        }

        public async Task<IActionResult> Index(int page = 1, string searchQuery = "")
        {
            int pageSize = 5; // Number of items per page

            // Create a filter expression that includes the search
            Expression<Func<Job, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filter = job => job.Title.Contains(searchQuery);
                // You can expand this to search other fields if needed
                // filter = job => job.Title.Contains(searchQuery) || job.Description.Contains(searchQuery);
            }

            // Get total count for pagination with the filter applied
            var totalJobs = await _unitOfWork.JobRepository.CountAsync(filter);

            // Get only the jobs for the current page with filter applied
            var jobs = await _unitOfWork.JobRepository.GetPagedListAsync(
                filter: filter,
                includeProperties: "Batch,Department",
                pageIndex: page - 1,
                pageSize: pageSize
            );

            var jobViewModels = jobs.Select(job => new JobViewM
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description,
                JobType = job.JobType,
                BatchId = job.BatchId,
                DepartmentId = job.DepartmentId,
                DepartmentName = job.Department?.Name
            }).ToList();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalJobs / (double)pageSize);

            // Pass data to the view
            ViewBag.TotalItems = totalJobs;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery; // Pass the search query to the view

            return View(jobViewModels);
        }



        // Load dropdowns for Create/Edit
        private async Task PopulateDropdowns(JobViewM model)
        {
            var departments = await _unitOfWork.DepRepository.GetAllAsync();
            

            model.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.Name
            }).ToList();

            
        }


        // Create - GET
        public async Task<IActionResult> Create()
        {
            var model = new JobViewM();
            await PopulateDropdowns(model);
            
            return View(model); // Return the populated model
        }

        // Create - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobViewM model)
        {

            if (ModelState.IsValid)
            {
                var job = _mapper.Map<Job>(model);
                await _unitOfWork.JobRepository.AddAsync(job);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model);

            return View(model);
        }

        // Edit - GET
        public async Task<IActionResult> Edit(int id)
        {
            var job = await _unitOfWork.JobRepository.GetFirstOrDefaultAsync(filter: J => J.JobId == id);
            if (job == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<JobViewM>(job);
            model.BatchId = job.BatchId;
            await PopulateDropdowns(model);
            
            return View(model);
        }

        // Edit - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, JobViewM model)
        {
            if (id != model.JobId) return NotFound();

            if (ModelState.IsValid)
            {
                var job = _mapper.Map<Job>(model);
                _unitOfWork.JobRepository.Update(job);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var job = await _unitOfWork.JobRepository.GetFirstOrDefaultAsync(filter: J => J.JobId == id);
            if (job == null) return NotFound();

            _unitOfWork.JobRepository.Remove(job);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
        #region Details
        //public async Task<IActionResult> Details(int id)
        //{
        //    var job = await _unitOfWork.JobRepository.GetFirstOrDefaultAsync(
        //        filter: j => j.JobId == id,
        //        includeProperties: "Batch,Department,JobApplications.AppUser"
        //    );

        //    if (job == null)
        //    {
        //        return NotFound();
        //    }

        //    var model = new JobViewModel
        //    {
        //        JobId = job.JobId,
        //        Title = job.Title,
        //        Description = job.Description,
        //        JobType = job.JobType,
        //        BatchId = job.BatchId,
        //        DepartmentId = job.DepartmentId,

        //        // Populate Batches and Departments lists
        //        Batches = (await _unitOfWork.BatchRepository.GetAllAsync()).Select(b => new SelectListItem
        //        {
        //            Value = b.BatchId.ToString(),
        //            Text = b.BatchName // Change this to match your Batch entity's name field
        //        }).ToList(),

        //        Departments = (await _unitOfWork.DepRepository.GetAllAsync()).Select(d => new SelectListItem
        //        {
        //            Value = d.DepartmentId.ToString(),
        //            Text = d.Name // Change this to match your Department entity's name field
        //        }).ToList(),

        //        JobApplications = job.JobApplications.Select(app => new JobApplicationViewModel
        //        {
        //            ApplicationId = app.ApplicationId,
        //            UserId = app.UserId,
        //            ApplicantName = app.AppUser.UserName,
        //            ApplicantEmail = app.AppUser.Email,
        //            AppliedDate = app.AppliedDate,
        //            Status = app.Status
        //        }).ToList()
        //    };

        //    return View(model);
        //}
        #endregion

        public async Task<IActionResult> Details(int id, string searchQuery = null)
        {
            var job = await _unitOfWork.JobRepository.GetFirstOrDefaultAsync(
                filter: j => j.JobId == id,
                includeProperties: "Batch,Department,JobApplications.AppUser"
            );

            if (job == null)
            {
                return NotFound();
            }

         
            

            var model = new JobViewM
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description,
                JobType = job.JobType,
                BatchId = job.BatchId,
                DepartmentId = job.DepartmentId,
                //Batches = (await _unitOfWork.BatchRepository.GetAllAsync()).Select(b => new SelectListItem
                //{
                //    Value = b.BatchId.ToString(),
                //    Text = b.BatchName
                //}).ToList(),
                Departments = (await _unitOfWork.DepRepository.GetAllAsync()).Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name
                }).ToList(),
            };

            return View(model);
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

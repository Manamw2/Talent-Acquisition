using AutoMapper;
using DataAccess.Repository.IRepository;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
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
        private readonly UserManager<AppUser> _userManager;
        public JobController(IUnitOfWork unitOfWork, IMapper mapper , UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 5; // Number of items per page
            int pageNumber = page ?? 1; // Default to page 1 if no page is specified

            var jobs = await _unitOfWork.JobRepository
                .GetAllAsync(includeProperties: "Batch,Department");

            var jobViewModels = jobs.Select(job => new JobViewModel
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description,
                JobType = job.JobType,
                BatchId = job.BatchId,
                DepartmentId = job.DepartmentId,
                BatchName = job.Batch?.BatchName,
                DepartmentName = job.Department?.Name
            }).ToList();

            var pagedJobs = jobViewModels.ToPagedList(pageNumber, pageSize);

            return View(pagedJobs);
        }



        // Load dropdowns for Create/Edit
        private async Task PopulateDropdowns(JobViewModel model)
        {
            var departments = await _unitOfWork.DepRepository.GetAllAsync();
            var batches = await _unitOfWork.BatchRepository.GetAllAsync(
                filter: b => (b.Job == null || b.BatchId == model.BatchId ) && b.EndDate >= DateTime.Now 
            );

            model.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.Name
            }).ToList();

            model.Batches = batches.Select(b => new SelectListItem
            {
                Value = b.BatchId.ToString(),
                Text = b.BatchName
            }).ToList();

            // Set the selected batch to the batch associated with the job
            if (model.BatchId != null)
            {
                var selectedBatch = model.Batches.FirstOrDefault(b => b.Value == model.BatchId.ToString());
                if (selectedBatch != null)
                {
                    selectedBatch.Selected = true;
                }
            }
        }


        // Create - GET
        public async Task<IActionResult> Create()
        {
            var model = new JobViewModel();
            await PopulateDropdowns(model);
            // Restore newly added batch selection
            if (TempData["NewBatchId"] != null)
            {
                model.BatchId = (int)TempData["NewBatchId"];
            }
            return View(model); // Return the populated model
        }

        // Create - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobViewModel model)
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

            var model = _mapper.Map<JobViewModel>(job);
            model.BatchId = job.BatchId;
            await PopulateDropdowns(model);
            
            return View(model);
        }

        // Edit - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, JobViewModel model)
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

            var applications = new List<JobApplicationViewModel>();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"http://localhost:8000/search?query={Uri.EscapeDataString(searchQuery)}&max_results=5&exact_thresh=0.9&nonexact_thresh=0.5");

                    if (response.IsSuccessStatusCode)
                    {
                        var searchResult = await response.Content.ReadFromJsonAsync<SearchResult>();
                        var matchedUserIds = searchResult.Results.Select(r => r.Id).ToList();

                        // Filter applications based on matched user IDs
                        applications = job.JobApplications
                            .Where(app => matchedUserIds.Contains(app.UserId))
                            .Select(app => new JobApplicationViewModel
                            {
                                ApplicationId = app.ApplicationId,
                                UserId = app.UserId,
                                ApplicantName = app.AppUser.UserName,
                                ApplicantEmail = app.AppUser.Email,
                                AppliedDate = app.AppliedDate,
                                Status = app.Status
                            }).ToList();
                    }
                }
            }
            else
            {
                applications = job.JobApplications.Select(app => new JobApplicationViewModel
                {
                    ApplicationId = app.ApplicationId,
                    UserId = app.UserId,
                    ApplicantName = app.AppUser.UserName,
                    ApplicantEmail = app.AppUser.Email,
                    AppliedDate = app.AppliedDate,
                    Status = app.Status
                }).ToList();
            }

            var model = new JobViewModel
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description,
                JobType = job.JobType,
                BatchId = job.BatchId,
                DepartmentId = job.DepartmentId,
                Batches = (await _unitOfWork.BatchRepository.GetAllAsync()).Select(b => new SelectListItem
                {
                    Value = b.BatchId.ToString(),
                    Text = b.BatchName
                }).ToList(),
                Departments = (await _unitOfWork.DepRepository.GetAllAsync()).Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name
                }).ToList(),
                JobApplications = applications
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



    }

}

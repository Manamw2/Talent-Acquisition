using AutoMapper;
using DataAccess.Repository.IRepository;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using System.Drawing.Printing;


namespace HrBackOffice.Controllers
{
    [Authorize]
    public class BatchController : Controller
	{
        private readonly IUnitOfWork _unitOfWork;

        public BatchController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(int page = 1)
		{
            int pageSize = 5;
			var batches = await _unitOfWork.BatchRepository.GetAllAsync();

			
            var paginatedJobs = batches.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Calculate total pages
            var totalJobs = batches.Count();
            var totalPages = (int)Math.Ceiling(totalJobs / (double)pageSize);

            // Pass data to the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(paginatedJobs);
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
        #region Comm
        //[HttpPost]
        //        [Route("Batch/CreateBatchAjax")]

        //        public async Task<IActionResult> CreateBatchAjax([FromBody] Batch batch)
        //        {
        //            if (batch == null || string.IsNullOrWhiteSpace(batch.BatchName))
        //            {
        //                return BadRequest("Invalid batch data");
        //            }

        //            await _unitOfWork.BatchRepository.AddAsync(batch);
        //            await _unitOfWork.SaveAsync();

        //            return Json(new { batchId = batch.BatchId, batchName = batch.BatchName });
        //        }
        #endregion




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
    }
}

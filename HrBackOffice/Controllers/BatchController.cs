using AutoMapper;
using DataAccess.Repository.IRepository;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Mvc;
using Models;


namespace HrBackOffice.Controllers
{
	public class BatchController : Controller
	{
        private readonly IUnitOfWork _unitOfWork;

        public BatchController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
		{
			var batches = await _unitOfWork.BatchRepository.GetAllAsync();

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
        [Route("Batch/CreateBatchAjax")]

        public async Task<IActionResult> CreateBatchAjax([FromBody] Batch batch)
        {
            if (batch == null || string.IsNullOrWhiteSpace(batch.BatchName))
            {
                return BadRequest("Invalid batch data");
            }

            await _unitOfWork.BatchRepository.AddAsync(batch);
            await _unitOfWork.SaveAsync();

            return Json(new { batchId = batch.BatchId, batchName = batch.BatchName });
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

    }
}

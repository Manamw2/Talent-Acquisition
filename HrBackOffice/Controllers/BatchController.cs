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

       

		public async Task<IActionResult> Delete(int id)
		{
			var batch = await _unitOfWork.BatchRepository.GetFirstOrDefaultAsync(filter: b => b.BatchId == id);
            if (batch == null)
            {
                return NotFound(); // Return 404 if the batch is not found
            }
            _unitOfWork.BatchRepository.Remove(batch);

			await _unitOfWork.SaveAsync();

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

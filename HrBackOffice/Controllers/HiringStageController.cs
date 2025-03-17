using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.Mappers;

namespace HrBackOffice.Controllers
{
    public class HiringStageController : Controller
    {
        private readonly IHiringStageRepo _hiringStageRepo;

        public HiringStageController(IHiringStageRepo hiringStageRepo)
        {
            _hiringStageRepo = hiringStageRepo;
        }
        public async Task<IActionResult> Index()
        {
            var stages = await _hiringStageRepo.GetAllAsync();
            return View(stages.Select(u => u.ToHiringStageDto()).ToList());
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }
    }
}

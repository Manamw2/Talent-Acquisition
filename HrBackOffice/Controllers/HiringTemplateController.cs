using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.Mappers;
using Models.ViewModels;
using System.Security.Claims;

namespace HrBackOffice.Controllers
{
    public class HiringTemplateController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHiringTemplateRepo _hiringTemplateRepo;
        private readonly IHiringStageRepo _hiringStageRepo;
        public HiringTemplateController(IUnitOfWork unitOfWork, IHiringTemplateRepo hiringTemplateRepo, IHiringStageRepo hiringStageRepo)
        {
            _hiringTemplateRepo = hiringTemplateRepo;
            _unitOfWork = unitOfWork;
            _hiringStageRepo = hiringStageRepo;
        }
        public async Task<IActionResult> Index()
        {
            var templates = await _hiringTemplateRepo.GetAllAsync();
            return View(templates.Select(u => u.ToHiringTemplateDto()).ToList());
        }

        public async Task<IActionResult> Create()
        {
            var HiringTemplateVM = new HiringTemplateVM();
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            var parameters = await _unitOfWork.HiringParameterRepo.GetAllAsync();
            var stages = await _hiringStageRepo.GetAllAsync();
            HiringTemplateVM.AvailableStages = stages.Select(u => u.ToHiringStageDto()).ToList();
            return View(HiringTemplateVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(HiringTemplateVM hiringTemplateVM)
        {
            if (!ModelState.IsValid)
            {
                return View(hiringTemplateVM);
            }
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var hiringTemplate = new HiringTemplate
            {
                Name = hiringTemplateVM.HiringTemplate.Name,
                Description = hiringTemplateVM.HiringTemplate.Name,
                AppUserId = userId,
                CreatedOn = DateTime.Now,
                HiringTemplateStages = hiringTemplateVM.TemplateStages.Select(u => new HiringTemplateStage
                {
                    StageId = u.StageId,
                    Occurrence = u.Occurance
                }).ToList(),
            };
            await _hiringTemplateRepo.CreateAsync(hiringTemplate);
            return RedirectToAction("Index");
        }
    }
}

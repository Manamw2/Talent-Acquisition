using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;
        public HiringTemplateController(IUnitOfWork unitOfWork, IHiringTemplateRepo hiringTemplateRepo, IHiringStageRepo hiringStageRepo, ApplicationDbContext context)
        {
            _hiringTemplateRepo = hiringTemplateRepo;
            _unitOfWork = unitOfWork;
            _hiringStageRepo = hiringStageRepo;
            _context = context;
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

        public async Task<IActionResult> Details(int id)
        {
            var template = await _context.hiringTemplates
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            // Get the stages associated with this template with their details
            var templateStages = await _context.HiringTemplateStages
                .Where(ts => ts.TemplateId == id)
                .Select(ts => new TemplateStageDetailsVM
                {
                    StageId = ts.StageId,
                    StageName = ts.HiringStage.Name,
                    Occurance = ts.Occurrence,
                    OutcomeType = ts.HiringStage.OutcomeType,
                    OutcomeTypeName = ts.HiringStage.OutcomeType.ToString(),
                    Parameters = ts.HiringStage.HiringStageParameters.Select(p => p.HiringParameter).Select(p => p.Name).ToList()
                })
                .OrderBy(ts => ts.Occurance)
                .ToListAsync();

            var viewModel = new HiringTemplateDetailsVM
            {
                HiringTemplate = template,
                TemplateStages = templateStages
            };

            return View(viewModel);
        }
    }
}

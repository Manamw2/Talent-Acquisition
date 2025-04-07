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
    public class HiringStageController : Controller
    {
        private readonly IHiringStageRepo _hiringStageRepo;
        private readonly IUnitOfWork _unitOfWork;

        public HiringStageController(IHiringStageRepo hiringStageRepo, IUnitOfWork unitOfWork)
        {
            _hiringStageRepo = hiringStageRepo;
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var stages = await _hiringStageRepo.GetAllAsync();
            return View(stages.Select(u => u.ToSimplifiedHiringStageDto()).ToList());
        }

        public async Task<IActionResult> Create()
        {
            HiringStageVM hiringStageVM = new HiringStageVM();
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            var parameters = await _unitOfWork.HiringParameterRepo.GetAllAsync();
            hiringStageVM.DepartmentsList = departments.Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.DepartmentId.ToString(),
            });
            hiringStageVM.ParametersList = parameters.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString(),
            });
            return View(hiringStageVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(HiringStageVM hiringStageVM)
        {
            if (!ModelState.IsValid)
            {
                return View(hiringStageVM);
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var hiringStage = new HiringStage
            {
                Name = hiringStageVM.HiringStage.Name,
                AppUserId = userId,
                CreatedOn = DateTime.Now,
                OutcomeType = hiringStageVM.HiringStage.OutcomeType,
                StageDepartmentNeeds = hiringStageVM.Departments.Select(u => new StageDepartmentNeed { DepartmentId = u.Id, EmployeesNeeded = u.NeededEmployees}).ToList(),
                HiringStageParameters = hiringStageVM.ParameterIds.Select(u => new HiringStageParameter { ParameterId = u }).ToList(),
                HiringStageOutcomes = new List<HiringStageOutcome> { 
                    new HiringStageOutcome
                    {
                        Name = "Accepted",
                        NotificationMessage = hiringStageVM.AcceptedNotification,
                        ApplicationStatus = hiringStageVM.AcceptedApplicationStatus,
                    },
                    new HiringStageOutcome
                    {
                        Name = "Rejected",
                        NotificationMessage = hiringStageVM.RejectedNotification,
                        ApplicationStatus = hiringStageVM.RejectedApplicationStatus,
                    }
                }
            };
            await _hiringStageRepo.CreateAsync(hiringStage);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var hiringStage = await _hiringStageRepo.GetByIdAsync(id);
            if (hiringStage == null)
            {
                return NotFound();
            }
            var hiringstageVm = new HiringStageDetailsVM
            {
                HiringStage = hiringStage,
                Outcomes = hiringStage.HiringStageOutcomes.Select(o => new OutcomeVm
                {
                    Id = o.Id,
                    Name = o.Name,
                    ApplicationStatus = o.ApplicationStatus,
                    NotificationMessage = o.NotificationMessage,
                }).ToList(),
                Parameters = hiringStage.HiringStageParameters.Select(p => p.HiringParameter).Select(p => new ParameterVm
                {
                    Name = p.Name,
                }).ToList(),
            };
            return View(hiringstageVm);
        }
    }
}

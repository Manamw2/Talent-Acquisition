using AutoMapper;
using DataAccess.Repository.IRepository;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Mvc;

namespace HrBackOffice.Controllers
{
    public class JobController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public JobController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var jobs = await _unitOfWork.JobRepository.GetAllAsync(includeProperties: "Batch");
            var jobViewModels = _mapper.Map<List<JobViewModel>>(jobs);
            return View(jobViewModels);
        }
    }
}

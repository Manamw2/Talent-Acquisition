using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrBackOffice.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class HiringParametersApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public HiringParametersApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() {
            return Ok(await _unitOfWork.HiringParameterRepo.GetAllWithDetailsAsync());
        }

    }
}

using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos.Stage;
using Models.Mappers;
using System.Security.Claims;

namespace HrBackOffice.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HiringStageApiController : ControllerBase
    {
        private readonly IHiringStageRepo _hiringStageRepo;
        public HiringStageApiController(IHiringStageRepo hiringStageRepo)
        {
            _hiringStageRepo = hiringStageRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var stages = await _hiringStageRepo.GetAllAsync();
            return Ok(stages.Select(u => u.ToHiringStageDto()).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
        {
            var hiringStage = await _hiringStageRepo.GetByIdAsync(id);
            if (hiringStage == null)
            {
                return NotFound();
            }
            return Ok(hiringStage);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] UpsertHiringStageDto upsertHiringStageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var hiringStage = upsertHiringStageDto.ToHiringStage(userId);
            await _hiringStageRepo.CreateAsync(hiringStage);
            return CreatedAtAction(nameof(GetAllAsync), new { id = hiringStage.Id }, hiringStage);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var hiringStage = await _hiringStageRepo.GetByIdAsync(id);
            if (hiringStage == null) { return NotFound(); }
            await _hiringStageRepo.DeleteAsync(hiringStage);
            return NoContent();
        }
    }
}

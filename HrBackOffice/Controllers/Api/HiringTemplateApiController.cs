using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos.Stage;
using Models.Dtos.Template;
using Models.Mappers;
using System.Security.Claims;
namespace HrBackOffice.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HiringTemplateApiController : ControllerBase
    {
        private readonly IHiringTemplateRepo _hiringTemplateRepo;
        public HiringTemplateApiController(IHiringTemplateRepo hiringTemplateRepo)
        {
            _hiringTemplateRepo = hiringTemplateRepo;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var hiringTemplates = await _hiringTemplateRepo.GetAllAsync();
            return Ok(hiringTemplates.Select(u => u.ToHiringTemplateDto()));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var hiringTemplate = await _hiringTemplateRepo.GetByIdAsync(id);
            if (hiringTemplate == null) return NotFound();
            return Ok(hiringTemplate);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] UpsertHiringTemplateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var hiringTemplate = dto.ToHiringTemplate(userId);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = hiringTemplate.Id }, hiringTemplate);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var HiringTemplate = await _hiringTemplateRepo.GetByIdAsync(id);
            if(HiringTemplate == null) return NotFound();
            await _hiringTemplateRepo.DeleteAsync(HiringTemplate);
            return NoContent();
        }
    }
}

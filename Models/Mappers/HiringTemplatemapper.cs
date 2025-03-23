using Models.Dtos.Stage;
using Models.Dtos.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Mappers
{
    public static class HiringTemplateMapper
    {
        public static HiringTemplateDto ToHiringTemplateDto(this HiringTemplate template)
        {
            return new HiringTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                CreatedOn = template.CreatedOn,
                AddedByName = template?.CreatedBy?.DisplayName ?? "",
            };
        }

        public static HiringTemplate ToHiringTemplate(this UpsertHiringTemplateDto templateDto, string userId)
        {
            return new HiringTemplate
            {
                AppUserId = userId,
                Name = templateDto.Name,
                CreatedOn = templateDto.CreatedOn,
            };
        }
    }
}

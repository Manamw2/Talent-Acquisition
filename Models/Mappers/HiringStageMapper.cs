using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Models.Dtos.Parameter;
using Models.Dtos.Stage;

namespace Models.Mappers
{
    public static class HiringStageMapper
    {
        public static HiringStageDto ToHiringStageDto(this HiringStage stage)
        {
            return new HiringStageDto
            {
                Id = stage.Id,
                StageName = stage.Name,
                MaxValue = stage.MaxValue,
                MinValue = stage.MinValue,
                OutcomeType = stage.OutcomeType,
                OutcomeSet = stage.HiringStageOutcomes.Select(u => u.Name).ToList() ?? new List<string>(), // Ensure no null ref,
                Parameters = stage.HiringStageParameters.Select(u => u.HiringParameter).Select(x => new HiringParameterDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    DataType = x.DataType
                }).ToList() ?? new List<HiringParameterDto>(), // Ensure no null ref,
                CreatedBy = stage?.CreatedBy?.DisplayName ?? "",
                CreatedOn = stage.CreatedOn,
                departmentNeedDtos = stage.StageDepartmentNeeds.Select(u => new DepartmentNeedDto
                {
                    DepartmentId = u.DepartmentId,
                    DepartmentNeed = u.EmployeesNeeded,
                }).ToList() ?? new List<DepartmentNeedDto>(),
            };
        }

        public static SimplifiedHiringStageDto ToSimplifiedHiringStageDto(this HiringStage stage)
        {
            return new SimplifiedHiringStageDto
            {
                Id = stage.Id,
                StageName = stage.Name,
                MaxValue = stage.MaxValue,
                MinValue = stage.MinValue,
                OutcomeType = stage.OutcomeType,
                CreatedBy = stage?.CreatedBy?.DisplayName ?? "",
                CreatedOn = stage.CreatedOn,
            };
        }

        public static HiringStage ToHiringStage(this UpsertHiringStageDto hiringStageDto, string userId)
        {
            return new HiringStage
            {
                AppUserId = userId,
                CreatedOn = DateTime.Now,
                OutcomeType = hiringStageDto.OutcomeType,
                MaxValue = hiringStageDto.MaxValue,
                MinValue = hiringStageDto.MinValue,
                Name = hiringStageDto.StageName,
                HiringStageParameters = hiringStageDto.ParametersIds.Select(u => new HiringStageParameter
                {
                    ParameterId = u
                }).ToList(),
                HiringStageOutcomes = hiringStageDto.OutcomeSet.Select(u => new HiringStageOutcome
                {
                    Name = u
                }).ToList(),
                StageDepartmentNeeds = hiringStageDto.departmentNeedDtos.Select(u => new StageDepartmentNeed
                {
                    DepartmentId = u.DepartmentId,
                    EmployeesNeeded = u.DepartmentNeed
                }).ToList(),
            };
        }
    }
}

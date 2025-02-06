using AutoMapper;
using HrBackOffice.Models;
using Models;

namespace HrBackOffice.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Job, JobViewModel>()
                .ForMember(dest => dest.BatchName, opt => opt.MapFrom(src => src.Batch.BatchName));

            CreateMap<Job, JobViewModel>()
           .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
           .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Batch.EndDate));
        }
    }
}

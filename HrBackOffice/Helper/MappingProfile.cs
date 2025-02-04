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
                .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src => src.BatchId))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));

            CreateMap<JobViewModel, Job>()
                .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src => src.BatchId))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));
        }
    }
}

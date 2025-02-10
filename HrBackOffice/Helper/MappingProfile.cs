using AutoMapper;
using Models;

//using HrBackOffice.Models;
using Models.ViewModels;

namespace HrBackOffice.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Job, JobViewModel>()
                .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src => src.BatchId))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Batch.EndDate))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));

            CreateMap<JobViewModel, Job>()
                .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src => src.BatchId))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));
        }
    }
}

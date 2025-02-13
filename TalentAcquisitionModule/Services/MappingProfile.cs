using AutoMapper;
using Models;
using Models.ViewModels;

namespace TalentAcquisitionModule.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<Job, JobViewModel>()
            //    .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src => src.BatchId))
            //    .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));

            //CreateMap<JobViewModel, Job>()
            //    .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src => src.BatchId))
            //    .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));

            CreateMap<Job, JobViewModel>()
           .ForMember(dest => dest.DepartmentName,
                     opt => opt.MapFrom(src => src.Department.Name))
           .ForMember(dest => dest.EndDate,
                     opt => opt.MapFrom(src => src.Batch.EndDate))
           .ForMember(dest => dest.ApplicationCount,
                     opt => opt.MapFrom(src => src.JobApplications.Count));
        }
    }
    }

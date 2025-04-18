﻿using AutoMapper;
using Models;

using HrBackOffice.Models;
using Models.Dtos.Template;
using Models.Dtos.Stage;
//using Models.ViewModels;

namespace HrBackOffice.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Job, JobViewM>()
                .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src => src.BatchId))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));

            CreateMap<JobViewM, Job>()
                .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src => src.BatchId))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));

            CreateMap<Batch, BatchViewModel>()
                .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId));

            CreateMap<BatchViewModel, Batch>()
                .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId));
        }
    }
}

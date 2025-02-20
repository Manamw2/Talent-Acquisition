﻿using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IRepository<Job> JobRepository { get; }
        IRepository<AppUser> UserRepository { get; }
        IRepository<Batch> BatchRepository { get; }
        IRepository<Department> DepartmentRepository { get; }
        IRepository<JobApplication> JobApplicationRepository { get; }
        IRepository<JobRecommend> JobRecommendRepository { get; }
        IRepository<Department> DepRepository { get; }
        IRepository<ApplicantExperience> AppExpRepository { get;  }
        IRepository<ApplicantSkill> AppSkillRepository { get;}
        IRepository<ApplicantProject> AppProjRepository { get; }
        Task SaveAsync();
    }
}

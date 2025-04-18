﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;


namespace DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IRepository<Job> JobRepository { get; private set; }
        public IRepository<AppUser> UserRepository { get; private set; }
        public IRepository<EmployeeTask> TaskRepository { get; private set; }
        public IRepository<Batch> BatchRepository { get; private set; }
        public IRepository<ApplicantExperience> AppExpRepository { get; private set; }
        public IRepository<ApplicantSkill> AppSkillRepository { get; private set; }
        public IRepository<ApplicantProject> AppProjRepository { get; private set; }

        public IRepository<JobApplication> JobApplicationRepository { get; private set; }

        public IRepository<Department> DepartmentRepository { get; private set; }
        public IRepository<Employee> EmpRepository { get; private set; }
        public IRepository<Department> DepRepository { get; private set; }

        public IRepository<JobRecommend> JobRecommendRepository { get; private set; }
        public IBulkCvsJobsHistoryRepo BulkCvsJobsHistoryRepo { get; private set; }
        public IRepository<HiringParameter> HiringParameterRepo { get; private set; }
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            JobRepository = new Repository<Job>(_context);
            UserRepository = new Repository<AppUser>(_context);
            TaskRepository = new Repository<EmployeeTask>(_context);
            BatchRepository = new Repository<Batch>(_context);
            JobApplicationRepository = new Repository<JobApplication>(_context);
            DepartmentRepository = new Repository<Department>(_context);
            EmpRepository = new Repository<Employee>(_context);
            DepRepository = new Repository<Department>(_context);
            AppExpRepository = new Repository<ApplicantExperience>(_context);
            AppSkillRepository = new Repository<ApplicantSkill>(_context);
            AppProjRepository = new Repository<ApplicantProject>(_context);
            JobRecommendRepository = new Repository<JobRecommend>(_context);
            BulkCvsJobsHistoryRepo = new BulkCvsJobsHistoryRepo(_context);
            HiringParameterRepo = new Repository<HiringParameter>(_context);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

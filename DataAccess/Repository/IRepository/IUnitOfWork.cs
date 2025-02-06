using Models;
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
        IRepository<Batch> BatchRepository { get; }
        IRepository<Department> DepartmentRepository { get; }
        public IRepository<JobApplication> JobApplicationRepository { get; }

        Task SaveAsync();
    }
}

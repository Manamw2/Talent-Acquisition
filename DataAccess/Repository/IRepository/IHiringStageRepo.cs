using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IHiringStageRepo
    {
        Task<List<HiringStage>> GetAllAsync();
        Task<HiringStage?> GetByIdAsync(int id);
        Task CreateAsync(HiringStage hiringStage);
        Task UpdateAsync(HiringStage hiringStage);
        Task DeleteAsync(HiringStage hiringStage);
    }
}

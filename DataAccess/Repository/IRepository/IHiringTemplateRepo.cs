using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IHiringTemplateRepo
    {
        Task<List<HiringTemplate>> GetAllAsync();
        Task<HiringTemplate?> GetByIdAsync(int id);
        Task CreateAsync(HiringTemplate hiringTemplate);
        Task UpdateAsync(HiringTemplate hiringTemplate);
        Task DeleteAsync(HiringTemplate hiringTemplate);
    }
}

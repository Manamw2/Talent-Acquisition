using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class HiringStageRepo : IHiringStageRepo
    {
        private readonly ApplicationDbContext _context;
        public HiringStageRepo(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task CreateAsync(HiringStage hiringStage)
        {
            await _context.hiringStages.AddAsync(hiringStage);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(HiringStage hiringStage)
        {
             _context.hiringStages.Remove(hiringStage);
            await _context.SaveChangesAsync();
        }

        public async Task<List<HiringStage>> GetAllAsync()
        {
            return await _context.hiringStages.Include(u => u.HiringStageParameters).ThenInclude(x => x.HiringParameter)
                .Include(u => u.HiringStageOutcomes).Include(u => u.CreatedBy).ToListAsync();
        }

        public async Task<HiringStage?> GetByIdAsync(int id)
        {
            return await _context.hiringStages
                .Include(u => u.CreatedBy)
                .Include(u => u.HiringStageOutcomes)
                .Include(u => u.HiringStageParameters).ThenInclude(x => x.HiringParameter)
                .Include(u => u.StageDepartmentNeeds).ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateAsync(HiringStage hiringStage)
        {
            _context.hiringStages.Update(hiringStage);
            await _context.SaveChangesAsync();
        }
    }
}

using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class HiringTemplateRepo : IHiringTemplateRepo
    {
        private readonly ApplicationDbContext _context;
        public HiringTemplateRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(HiringTemplate hiringTemplate)
        {
            await _context.hiringTemplates.AddAsync(hiringTemplate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(HiringTemplate hiringTemplate)
        {
            _context.hiringTemplates.Remove(hiringTemplate);
            await _context.SaveChangesAsync();
        }

        public async Task<List<HiringTemplate>> GetAllAsync()
        {
            return await _context.hiringTemplates
                .Include(u => u.CreatedBy)
                .Include(u => u.HiringTemplateStages).ThenInclude(x => x.HiringStage)
                .ToListAsync();
        }

        public async Task<HiringTemplate?> GetByIdAsync(int id)
        {
            return await _context.hiringTemplates
                .Include(u => u.HiringTemplateStages).ThenInclude(x => x.HiringStage)
                .FirstOrDefaultAsync(u =>  u.Id == id);
        }

        public async Task UpdateAsync(HiringTemplate hiringTemplate)
        {
            _context.hiringTemplates.Update(hiringTemplate);
            await _context.SaveChangesAsync();
        }
    }
}

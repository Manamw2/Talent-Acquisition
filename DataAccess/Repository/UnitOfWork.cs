using System;
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
        public IRepository<Batch> BatchRepository { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            JobRepository = new Repository<Job>(_context);
            BatchRepository = new Repository<Batch>(_context);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}

using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class BulkCvsJobsHistoryRepo : Repository<BulkCvsJobsHistory>, IBulkCvsJobsHistoryRepo
    {
        public BulkCvsJobsHistoryRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}

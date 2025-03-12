using Models;

namespace HrBackOffice.Models
{
    public class DepartmentDetailsVM
    {
        public Department Department { get; set; }
        public List<Employee> Employees { get; set; }
    }
}

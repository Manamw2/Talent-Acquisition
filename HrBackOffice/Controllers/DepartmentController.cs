using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using DataAccess.Repository.IRepository;
using System.Linq.Expressions;
using HrBackOffice.Models;

namespace HrBackOffice.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Department
        
        public async Task<IActionResult> Index(int page = 1, string searchQuery = "")
        {
            int pageSize = 5;
// Create a filter expression that includes the search
            Expression<Func<Department, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filter = Dep => Dep.Name.Contains(searchQuery);
                // You can expand this to search other fields if needed
                // filter = job => job.Title.Contains(searchQuery) || job.Description.Contains(searchQuery);
            }
            // Get total count for pagination
            var totalBatches = await _unitOfWork.DepartmentRepository.CountAsync(filter);

            // Get only the Depts for the current page
            var depts = await _unitOfWork.DepartmentRepository.GetPagedListAsync(
                filter: filter,
                pageIndex: page - 1,
                pageSize: pageSize
            );
            
            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalBatches / (double)pageSize);

            // Pass data to the view
            ViewBag.TotalItems = totalBatches;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery;
            return View(depts);
        }
        // GET: Department/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the department
            var department = await _unitOfWork.DepartmentRepository.GetFirstOrDefaultAsync(m => m.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }

            // Get employees in this department separately
            var employees = await _unitOfWork.EmpRepository.GetAllAsync(
                filter: e => e.DepartmentId == id
            );

            var departmentViewModel = new DepartmentDetailsVM
            {
                Department = department,
                Employees = employees.ToList()
            };

            return View(departmentViewModel);
        }

        // GET: Department/Create
        public IActionResult Create()
        {
            return View(new Department());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.DepartmentRepository.AddAsync(department);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Department/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _unitOfWork.DepartmentRepository.GetFirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Department/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,Name")] Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.DepartmentRepository.Update(department);
                    await _unitOfWork.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    
                        throw;
                    
                }
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var department = await _unitOfWork.DepartmentRepository.GetFirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department != null)
            {
                _unitOfWork.DepartmentRepository.Remove(department);
                
            }
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
      
        //private bool DepartmentExists(int id)
        //{
        //    return _context.Department.Any(e => e.DepartmentId == id);
        //}
    }
}

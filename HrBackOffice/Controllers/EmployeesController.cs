using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using Models;
using System.Linq.Expressions;
using DataAccess.Repository.IRepository;

namespace HrBackOffice.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        public EmployeesController( IUnitOfWork unitOfWork,ApplicationDbContext context )
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int page = 1, string searchQuery = "")
        {
            int pageSize = 5;
            // Create a filter expression that includes the search
            Expression<Func<Employee, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filter = Emp => Emp.Name.Contains(searchQuery);
                
            }
            // Get total count for pagination
            var totalEmps = await _unitOfWork.EmpRepository.CountAsync(filter);

            // Get only the Depts for the current page
            var Emps = await _unitOfWork.EmpRepository.GetPagedListAsync(
                filter: filter,
                includeProperties:"Department",
                pageIndex: page - 1,
                pageSize: pageSize
            );

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalEmps / (double)pageSize);

            // Pass data to the view
            ViewBag.TotalItems = totalEmps;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery;
            return View(Emps);
        }
        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.EmpId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name");
            return View(new Employee());
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.EmpRepository.AddAsync(employee);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", employee.DepartmentId);
            return View(employee);
        }
        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", employee.DepartmentId);
            return View(employee);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmpId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.EmpRepository.Update(employee);
                    await _unitOfWork.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmpId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", employee.DepartmentId);
            return View(employee);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _unitOfWork.EmpRepository.GetFirstOrDefaultAsync(e=> e.EmpId == id);
            if (employee != null)
            {
                _unitOfWork.EmpRepository.Remove(employee);
            }

            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employee.Any(e => e.EmpId == id);
        }
    }
}

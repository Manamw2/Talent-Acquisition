using DataAccess.Data;
using DataAccess.Repository.IRepository;
using HrBackOffice.Extentions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using System.Linq.Expressions;

namespace HrBackOffice.Controllers
{
    public class HiringTasksController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public HiringTasksController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string? searchTerm = null, string? status = null, string? department = null, string? batch = null)
        {
            // Get current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Get the employee associated with the current user
            var employee = await _unitOfWork.EmpRepository.GetFirstOrDefaultAsync(e => e.Email == user.Email);
            if (employee == null) return NotFound();

            // Get tasks assigned to the employee
            var tasksViewModel = await GetHiringTasksForEmployee(employee.EmpId, page, pageSize, searchTerm, status, department, batch);

            return View(tasksViewModel);
        }

        private async Task<HiringTasksViewModel> GetHiringTasksForEmployee(int employeeId, int page,
            int pageSize, string? searchTerm, string? status, string? department, string? batch)
        {
            // Build the filter expression
            Expression<Func<EmployeeTask, bool>> filter = e => e.EmployeeId == employeeId;

            // Add optional filters
            if (!string.IsNullOrEmpty(status))
            {
                filter = filter.And(e => e.Status == status);
            }

            if (!string.IsNullOrEmpty(department) && int.TryParse(department, out int departmentId))
            {
                filter = filter.And(e => e.HiringTask.Batch.Job.Department.DepartmentId == departmentId);
            }

            if (!string.IsNullOrEmpty(batch) && int.TryParse(batch, out int batchId))
            {
                filter = filter.And(e => e.HiringTask.BatchId == batchId);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filter = filter.And(e => e.HiringTask.Batch.BatchName.Contains(searchTerm) ||
                                        e.HiringTask.HiringStage.Name.Contains(searchTerm));
            }

            // Include related entities
            string includeProperties = "HiringTask,HiringTask.HiringStage,HiringTask.Batch,HiringTask.Batch.Job,HiringTask.Batch.Job.Department";

            // Get paged list of tasks
            var employeeTasks = await _unitOfWork.TaskRepository.GetPagedListAsync(
                filter,
                orderBy: q => q.OrderByDescending(t => t.AssignedDate),
                includeProperties: includeProperties,
                pageIndex: page - 1,
                pageSize: pageSize);

            // Count total tasks for pagination
            var totalTasks = await _unitOfWork.TaskRepository.CountAsync(filter);

            // Get all unique statuses, departments, and batches for filter dropdowns
            var allEmployeeTasks = await _unitOfWork.TaskRepository.GetAllAsync(
                filter: e => e.EmployeeId == employeeId,
                includeProperties: "HiringTask.Batch.Job.Department");

            var statuses = new List<string> { "New", "Active", "Hold", "Completed" };
            var departments = allEmployeeTasks.Select(t => t.HiringTask.Batch.Job.Department).DistinctBy(d => d.DepartmentId).ToList();
            var batches = allEmployeeTasks.Select(t => t.HiringTask.Batch).DistinctBy(b => b.BatchId).ToList();

            // Create view model
            var viewModel = new HiringTasksViewModel
            {
                Tasks = employeeTasks.ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalTasks = totalTasks,
                TotalPages = (int)Math.Ceiling(totalTasks / (double)pageSize),
                SearchTerm = searchTerm,
                Status = status,
                Department = department,
                Batch = batch,
                Statuses = statuses,
                Departments = departments,
                Batches = batches
            };

            return viewModel;
        }

        // Task details action
        public async Task<IActionResult> Details(int id)
        {
            var employeeTask = await _unitOfWork.TaskRepository.GetFirstOrDefaultAsync(
                e => e.Id == id,  // Changed from EmployeeId to Id
                includeProperties: "HiringTask,HiringTask.HiringStage,HiringTask.Batch,HiringTask.Batch.Job,HiringTask.Batch.Job.Department,HiringTask.HiringStage.HiringStageParameters"
            );

            if (employeeTask == null) return NotFound();

            // Get applicants for this batch
            var applicants = await _unitOfWork.JobApplicationRepository.GetAllAsync(
                a => a.BatchId == employeeTask.HiringTask.BatchId,
                includeProperties: "AppUser,Job"
            );

            var viewModel = new TaskDetailsViewModel
            {
                EmployeeTask = employeeTask,
                Applicants = applicants.ToList()
            };

            return View(viewModel);
        }

        // Update task status
        public async Task<IActionResult> UpdateTaskStatus(int id, string status)
        {
            var employeeTask = await _unitOfWork.TaskRepository.GetFirstOrDefaultAsync(e => e.Id == id);
            if (employeeTask == null) return NotFound();

            // Validate status
            if (status != "New" && status != "Active" && status != "Hold" && status != "Completed")
            {
                ModelState.AddModelError("", "Invalid status value");
                return RedirectToAction("Details", new { id = id });
            }

            // Update status
            employeeTask.Status = status;
            _unitOfWork.TaskRepository.Update(employeeTask);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", new { id = id });
        }

        // Update applicant status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int applicantId, int taskId, string status, string comments)
        {
            var application = await _unitOfWork.JobApplicationRepository.GetFirstOrDefaultAsync(a => a.ApplicationId == applicantId);
            if (application == null) return NotFound();

            // Update the status
            application.Status = status;

            
            _unitOfWork.JobApplicationRepository.Update(application);

            // Update task status to Active when processing applicants
            var employeeTask = await _unitOfWork.TaskRepository.GetFirstOrDefaultAsync(e => e.Id == taskId);
            if (employeeTask != null && employeeTask.Status == "New")
            {
                employeeTask.Status = "Active";
                _unitOfWork.TaskRepository.Update(employeeTask);
            }

            await _unitOfWork.SaveAsync();

            return RedirectToAction("Details", new { id = taskId });
        }
    }
}


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
            if (user == null) return RedirectToAction("Login", "Admin");

            // Get the employee associated with the current user
            var employee = await _unitOfWork.EmpRepository.GetFirstOrDefaultAsync(e => e.Email == user.Email);

            // Check if the user is not an employee
            if (employee == null)
            {
                bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (isAdmin)
                {
                    // Redirect to the admin dashboard
                    return RedirectToAction("Dashboard", "Admin");
                }

                // If the user doesn't have appropriate roles, show access denied
                return RedirectToAction("AccessDenied", "Admin");
            }

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

            // Handle task assignment
            if (employeeTask.Status == "UnAssigned" && status == "Assigned")
            {
                try
                {
                    // Mark task as assigned for current employee
                    employeeTask.isAssigned = true;
                    employeeTask.Status = "New"; // Automatically move to New status
                    employeeTask.AssignedDate = DateTime.Now;

                    // Update this employee's task
                    _unitOfWork.TaskRepository.Update(employeeTask);
                    await _unitOfWork.SaveAsync();

                    // Now remove task from other employees' lists
                    var taskId = employeeTask.TaskId;
                    var currentEmployeeId = employeeTask.EmployeeId;

                    var otherEmployeeTasks = await _unitOfWork.TaskRepository.GetAllAsync(
                        e => e.TaskId == taskId && e.EmployeeId != currentEmployeeId);

                    foreach (var task in otherEmployeeTasks)
                    {
                        _unitOfWork.TaskRepository.Remove(task);
                    }

                    await _unitOfWork.SaveAsync();

                    return RedirectToAction("Details", new { id = id });
                }
                catch (Exception ex)
                {
                    // Log exception
                    ModelState.AddModelError("", "Error occurred while assigning task");
                    return RedirectToAction("Details", new { id = id });
                }
            }

            // Regular status updates
            else if (employeeTask.isAssigned)
            {
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
            }

            return RedirectToAction("Details", new { id = id });
        }

        // Update applicant status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int applicantId, int taskId, string status, string comments)
        {
            // First, retrieve the application
            var application = await _unitOfWork.JobApplicationRepository.GetFirstOrDefaultAsync(a => a.ApplicationId == applicantId);
            if (application == null)
            {
                TempData["Error"] = "Application not found";
                return RedirectToAction("Details", new { id = taskId });
            }

            // Save the original status to check if it changed
            var originalStatus = application.Status;

            // Update the status
            application.Status = status;

            // Add comments if provided
            if (!string.IsNullOrEmpty(comments))
            {
               
                application.SourceDetails = comments; 
            }

            // Update the application
            _unitOfWork.JobApplicationRepository.Update(application);

            // Update task status to Active if it was New
            var employeeTask = await _unitOfWork.TaskRepository.GetFirstOrDefaultAsync(e => e.Id == taskId);
            if (employeeTask != null && employeeTask.Status == "New")
            {
                employeeTask.Status = "Active";
                _unitOfWork.TaskRepository.Update(employeeTask);
            }

            // Save changes
            await _unitOfWork.SaveAsync();

            // Add a success message
            TempData["Success"] = $"Status updated to {status} for applicant {application.AppUser?.DisplayName} ";

            // Redirect back to the task details page
            return RedirectToAction("Details", new { id = taskId });
        }
    }
}


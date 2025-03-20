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
using Microsoft.AspNetCore.Identity;
using HrBackOffice.Helper.EmailSetting;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace HrBackOffice.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSend _emailService;

        public EmployeesController(ApplicationDbContext context, IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IEmailSend emailService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailService = emailService;
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
                try
                {
                    // 1. Create the employee record
                    await _unitOfWork.EmpRepository.AddAsync(employee);
                    await _unitOfWork.SaveAsync();

                    // 2. Create corresponding user account
                    var user = new AppUser
                    {
                        UserName = employee.Email,
                        Email = employee.Email,
                        DisplayName = employee.Name,
                        EmailConfirmed = true
                    };

                    // Generate a random secure password
                    var password = GenerateSecureRandomPassword();

                    // Create the user account
                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        // 3. Assign default role
                        await _userManager.AddToRoleAsync(user, "Employee");

                        // 4. Generate password reset token and link
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        //token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                        var resetLink = Url.Action("ResetPassword", "Admin",
                        new { email = user.Email, token = token },
                        Request.Scheme);

                        // 5. Send password reset email
                        await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // If user creation fails, remove the employee
                        _unitOfWork.EmpRepository.Remove(employee);
                        await _unitOfWork.SaveAsync();

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }

            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", employee.DepartmentId);
            return View(employee);
        }

        // Helper method to generate a secure random password
        private string GenerateSecureRandomPassword()
        {
            const string upperChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            const string lowerChars = "abcdefghijkmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string specialChars = "!@#$%^&*()_-+=<>?";

            // Ensure password meets complexity requirements
            var random = new Random();
            var password = new StringBuilder();

            // Add at least one of each character type
            password.Append(upperChars[random.Next(upperChars.Length)]);
            password.Append(lowerChars[random.Next(lowerChars.Length)]);
            password.Append(numbers[random.Next(numbers.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);

            // Fill the rest with random characters
            const string allChars = upperChars + lowerChars + numbers + specialChars;
            for (int i = 4; i < 16; i++) // 16-character password
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password
            return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
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
                    // Get the old email before updating
                    var existingEmployee = await _context.Employee.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.EmpId == id);
                    string oldEmail = existingEmployee?.Email;

                    // Update employee
                    _unitOfWork.EmpRepository.Update(employee);
                    await _unitOfWork.SaveAsync();

                    // Update the associated user if email has changed
                    if (oldEmail != employee.Email && !string.IsNullOrEmpty(oldEmail))
                    {
                        var user = await _userManager.FindByEmailAsync(oldEmail);
                        if (user != null)
                        {
                            // Update basic user info
                            user.UserName = employee.Email;
                            user.Email = employee.Email;
                            user.DisplayName = employee.Name;

                            var updateResult = await _userManager.UpdateAsync(user);
                            if (!updateResult.Succeeded)
                            {
                                foreach (var error in updateResult.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                                ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", employee.DepartmentId);
                                return View(employee);
                            }
                        }
                    }
                    else
                    {
                        // If email didn't change, just update the display name
                        var user = await _userManager.FindByEmailAsync(employee.Email);
                        if (user != null && user.DisplayName != employee.Name)
                        {
                            user.DisplayName = employee.Name;
                            await _userManager.UpdateAsync(user);
                        }
                    }

                    TempData["SuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction(nameof(Index));
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
            }

            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", employee.DepartmentId);
            return View(employee);
        }



        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _unitOfWork.EmpRepository.GetFirstOrDefaultAsync(e=> e.EmpId == id);
            if (employee != null)
            {
                var user = await _userManager.FindByEmailAsync(employee.Email);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
               
                _unitOfWork.EmpRepository.Remove(employee);
                // Remove user from database
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest("Error deleting the user.");
                }
                
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

using HrBackOffice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HrBackOffice.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            // Fetch all users first
            var users = await _userManager.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                })
                .ToListAsync();

            // Fetch roles for each user asynchronously and filter those with "HR Admin" role
            var filteredUsers = new List<UserViewModel>();

            foreach (var user in users)
            {
                var userEntity = await _userManager.FindByIdAsync(user.Id);
                var roles = await _userManager.GetRolesAsync(userEntity);

                if (roles.Contains("HR") || roles.Contains("Admin")) // Check if the user has "HR Admin" role
                {
                    user.Roles = roles;
                    filteredUsers.Add(user);
                }
            }

            return View(filteredUsers);
        }


        public IActionResult Create()
        {
            var viewModel = new HRUserViewModel
            {
                Roles = _roleManager.Roles
                        .Where(r => r.Name == "HR" || r.Name == "Admin") // Filter HR and Admin roles
                        .Select(r => new SelectListItem
                        {
                            Value = r.Name,  // Role name as value
                            Text = r.Name     // Display role name as text
                        })
                        .ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(HRUserViewModel model)
        {
            // Initialize roles for the model in case we need to return to the view
            model.Roles = _roleManager.Roles
                .Where(r => r.Name == "HR" || r.Name == "Admin")
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if user already exists by email
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                TempData["AlertMessage"] = "A user with this email already exists.";
                TempData["AlertType"] = "danger";
                return View(model);
            }

            // Check if username already exists
            var username = model.Email.Split('@')[0];
            var existingUserByUsername = await _userManager.FindByNameAsync(username);
            if (existingUserByUsername != null)
            {
                ModelState.AddModelError("Email", "A user with this username already exists.");
                TempData["AlertMessage"] = "A user with this username already exists.";
                TempData["AlertType"] = "danger";
                return View(model);
            }

            // Create the user if validation passes
            var user = new AppUser
            {
                UserName = username,
                DisplayName = model.DisplayName,
                Email = model.Email,
                EmailConfirmed = true // Optional: set email as confirmed
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                }

                TempData["AlertMessage"] = "User created successfully!";
                TempData["AlertType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var allRoles = await _roleManager.Roles.ToListAsync();

            // Get the user's current role (if any)
            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRoleName = userRoles.FirstOrDefault();

            var viewModel = new UserRoleViewModel()
            {
                DisplayName = user.DisplayName,
                UserId = user.Id,
                UserName = user.UserName,
                Roles = allRoles.Select(r => new RoleViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSelected = r.Name == currentRoleName
                }).ToList()
            };

            // Set the selected role ID if the user has a role
            if (!string.IsNullOrEmpty(currentRoleName))
            {
                var currentRole = allRoles.FirstOrDefault(r => r.Name == currentRoleName);
                if (currentRole != null)
                {
                    viewModel.SelectedRoleId = currentRole.Id;
                }
            }

            return View(viewModel);
        }

        // Modified POST action for single role assignment
        [HttpPost]
        public async Task<IActionResult> Edit(string id, UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            // Get current user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Remove all existing roles
            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            // Add the selected role
            if (!string.IsNullOrEmpty(model.SelectedRoleId))
            {
                // Find the role name based on the selected ID
                var selectedRole = await _roleManager.FindByIdAsync(model.SelectedRoleId);
                if (selectedRole != null)
                {
                    await _userManager.AddToRoleAsync(user, selectedRole.Name);
                }
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid user ID.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Ensure the user is HR or Admin before deleting
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("HR") && !roles.Contains("Admin"))
            {
                return BadRequest("You can only delete HR or Admin users.");
            }

            // Remove user from database
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Error deleting the user.");
            }

            return Ok(new { success = true, message = "User deleted successfully!" });
        }

    }
}

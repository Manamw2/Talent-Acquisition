using HrBackOffice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Data;
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
                    Email = u.Email,
                })
                .ToListAsync();

            // Fetch roles for each user asynchronously and filter those with "HR Admin" role
            var filteredUsers = new List<UserViewModel>();

            foreach (var user in users)
            {
                var userEntity = await _userManager.FindByIdAsync(user.Id);
                var roles = await _userManager.GetRolesAsync(userEntity);

                if (roles.Contains("HR")) // Check if the user has "HR Admin" role
                {
                    user.Roles = roles;
                    filteredUsers.Add(user);
                }
            }

            return View(filteredUsers);
        }


        public IActionResult Create()
        {
            var viewModel = new UserViewModel
            {
                Roless = _roleManager.Roles
                        .Where(r => r.Name == "HR" || r.Name == "Admin") // Filter HR and Admin roles
                        .Select(r => new SelectListItem
                        {
                            Value = r.Name,  // Set role name as value
                            Text = r.Name     // Display role name as text
                        })
                        .ToList() // Convert to List<SelectListItem>
                            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Validation Errors: " + string.Join(", ", errors)); // Debugging
                return View(model);
            }

            // Extract username from email (before @ symbol)
            var username = model.Email.Split('@')[0];

            var user = new AppUser
            {
                DisplayName = model.DisplayName,
                UserName = username, // Automatically set username
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (model.Role != null)
                {
                    if (model.Role == "HR" || model.Role == "Admin") // Ensure only HR and Admin roles are assigned
                    {
                        if (await _roleManager.RoleExistsAsync(model.Role))
                        {
                            await _userManager.AddToRoleAsync(user, model.Role);
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }




        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var viewModel = new UserRoleViewModel()
            {
                DisplayName = user.DisplayName,
                UserId = user.Id,
                UserName = user.UserName,
                Roles = allRoles.Select(r => new RoleViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSelected = _userManager.IsInRoleAsync(user, r.Name).Result
                }).ToList()
            };
            return View(viewModel); 
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in model.Roles)
            {
                if (userRoles.Any(r => r == role.Name) && !role.IsSelected)
                    await _userManager.RemoveFromRoleAsync(user, role.Name);

                if (!userRoles.Any(r => r == role.Name) && role.IsSelected)
                    await _userManager.AddToRoleAsync(user, role.Name);
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

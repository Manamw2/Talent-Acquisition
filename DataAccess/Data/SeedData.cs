﻿using Microsoft.AspNetCore.Identity;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Initialize Roles
            string[] roleNames = { "hr", "applicant" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Add HR user
            var hrUser = await userManager.FindByEmailAsync("hr@gmail.com");
            if (hrUser == null)
            {
                hrUser = new AppUser
                {
                    UserName = "HRManager",
                    Email = "hr@gmail.com"
                };
                await userManager.CreateAsync(hrUser, "Hr@623145");
                await userManager.AddToRoleAsync(hrUser, "hr");
            }

            // Add Applicant user
            var applicantUser = await userManager.FindByEmailAsync("applicant@gmail.com");
            if (applicantUser == null)
            {
                applicantUser = new AppUser
                {
                    UserName = "Applicant1",
                    Email = "applicant@gmail.com"
                };
                await userManager.CreateAsync(applicantUser, "Applicant@623145");
                await userManager.AddToRoleAsync(applicantUser, "applicant");
            }
        }
    }
}

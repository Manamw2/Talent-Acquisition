using HrBackOffice.Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Mappers
{
    public static class HRResumeToProfileInfoMapper
    {
        public static UserViewModel HRResumeToProfileInfo(this ResumeViewModel source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source ResumeViewModel cannot be null.");
            }

            var target = new UserViewModel
            {
                // Map direct properties
                DisplayName = source.Name ?? string.Empty,
                Email = source.Email ?? string.Empty,
                Phone = source.Phone ?? string.Empty,

                // Map hidden relationships
                Faculty = source.Education?.FirstOrDefault()?.Institution ?? string.Empty,

                // Set default values for fields that don't have a direct mapping
                EnglishProficiencyLevel = string.Empty,
                MethodOfContact = string.Empty,
                BirthDate = null,
                CvUrl = string.Empty,
                CvFile = null,

                // Map nested collections
                ApplicantExperiences = source.Experience?.Select(e => new AppExperienceViewModel
                {
                    Company = e.Company ?? string.Empty,
                    Position = e.Position ?? string.Empty,
                    Description = e.Responsibilities[0] ?? string.Empty,
                    StartDate = null, // Default value since ResumeViewModel doesn't provide this
                    EndDate = null,    // Default value since ResumeViewModel doesn't provide this
                }).ToList() ?? new List<AppExperienceViewModel>(),

                ApplicantSkills = source.Skills?.Select(s => new AppSkillViewModel
                {
                    Name = s ?? string.Empty,
                    Level = string.Empty // Default value since ResumeViewModel doesn't provide this
                }).ToList() ?? new List<AppSkillViewModel>(),

                ApplicantProjects = source.Projects?.Select(p => new AppProjectViewModel
                {
                    Name = p.Title ?? string.Empty,
                    Description = p.Description ?? string.Empty
                }).ToList() ?? new List<AppProjectViewModel>()
            };

            return target;
        }
    }
}
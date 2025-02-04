using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ProfileCompletionVM
    {
        public List<ApplicantExperience> ApplicantExperiences = new List<ApplicantExperience>();
        public List<ApplicantSkill> ApplicantSkills = new List<ApplicantSkill>();
        public List<ApplicantProject> ApplicantProjects = new List<ApplicantProject>();

    }
}

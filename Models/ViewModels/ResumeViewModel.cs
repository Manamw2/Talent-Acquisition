using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class ResumeViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<Experience> Experience { get; set; }
        public List<Education> Education { get; set; }
        public List<string> Skills { get; set; }
        public List<Project> Projects { get; set; }
        public List<string> ExtracurricularActivities { get; set; }
    }

    public class Experience
    {
        public string Position { get; set; }
        public string Company { get; set; }
        [JsonPropertyName("Start Date")]
        public string StartDate { get; set; }
        [JsonPropertyName("End Date")]
        public string EndDate { get; set; }
        public List<string> Responsibilities { get; set; }
    }

    public class Education
    {
        [JsonPropertyName("University")]
        public string Institution { get; set; }
        public string Degree { get; set; }
        public string Duration { get; set; }
    }

    public class Project
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}

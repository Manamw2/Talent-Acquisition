using HrBackOffice.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Mappers;
using Models.ViewModels;
using System.Net.Http;
using System.Text.Json;

namespace HrBackOffice.Helper.ApplicantService
{
    public class ApplicantService : IApplicantService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public ApplicantService(HttpClient httpClient , IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<UserViewModel> ExtractDataFromCv(IFormFile cvFile)
        {
            var content = new MultipartFormDataContent();
            // Read the file content from cvFile into a MemoryStream
            using (var memoryStream = new MemoryStream())
            {
                await cvFile.CopyToAsync(memoryStream); // Copy the file content to the MemoryStream
                memoryStream.Position = 0; // Reset the stream position to the beginning

                // Create ByteArrayContent from the MemoryStream
                var fileContentStream = new ByteArrayContent(memoryStream.ToArray());

                // Set the Content-Disposition header
                fileContentStream.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "file", // Name of the form field
                    FileName = cvFile.FileName // Name of the file
                };

                // Set the Content-Type header based on the file type
                fileContentStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(cvFile.ContentType);

                // Add the file content to the MultipartFormDataContent
                content.Add(fileContentStream);
                var baseURl = _configuration["CVModelURl"]; 
                // Make the API call
                var response = await _httpClient.PostAsync(baseURl, content);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                ResumeViewModel resumeViewModel = JsonSerializer.Deserialize<ResumeViewModel>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var userViewModel = resumeViewModel.HRResumeToProfileInfo();
                return userViewModel;
            }
        }

        public void PopulateDropdownLists(UserViewModel model)
        {
            // Initialize Universities list
            IEnumerable<string> Universities = new List<string>
    {
        "Cairo University", "Ain Shams University", "Alexandria University", "Helwan University",
        "Mansoura University", "Zagazig University", "Assiut University", "Tanta University",
        "Benha University", "Suez Canal University", "Minia University", "South Valley University",
        "Fayoum University", "Beni-Suef University", "Sohag University", "Kafr El Sheikh University",
        "Damietta University", "Port Said University", "Menoufia University", "Al-Azhar University",
        "The British University in Egypt (BUE)", "The American University in Cairo (AUC)",
        "German University in Cairo (GUC)", "Misr University for Science and Technology (MUST)",
        "Future University in Egypt (FUE)", "October 6 University", "Modern Sciences and Arts University (MSA)",
        "Nahda University", "Sinai University"
    };

            // Initialize Faculties list
            IEnumerable<string> Faculties = new List<string>
    {
        "Faculty of Engineering", "Faculty of Medicine", "Faculty of Pharmacy", "Faculty of Science",
        "Faculty of Commerce", "Faculty of Law", "Faculty of Arts", "Faculty of Education",
        "Faculty of Agriculture", "Faculty of Dentistry", "Faculty of Computer and Artificial Intelligence",
        "Faculty of Veterinary Medicine", "Faculty of Nursing", "Faculty of Physical Therapy",
        "Faculty of Tourism and Hotels", "Faculty of Mass Communication", "Faculty of Fine Arts",
        "Faculty of Applied Arts", "Faculty of Al-Alsun (Languages)", "Faculty of Islamic Studies"
    };

            // Initialize lists before inserting
            model.Universities = Universities.Select(u => new SelectListItem { Value = u, Text = u }).ToList();
            model.Universities.Insert(0, new SelectListItem { Value = "", Text = "Select University" });
            model.Universities.Insert(1, new SelectListItem { Value = "Other", Text = "Other" });

            model.Faculties = Faculties.Select(f => new SelectListItem { Value = f, Text = f }).ToList();
            model.Faculties.Insert(0, new SelectListItem { Value = "", Text = "Select Faculty" });
            model.Faculties.Insert(1, new SelectListItem { Value = "Other", Text = "Other" });

            // Initialize EducationLevels before inserting
            model.EducationLevels = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "Select Education Level" },
        new SelectListItem { Value = "High School", Text = "High School" },
        new SelectListItem { Value = "Bachelor", Text = "Bachelor" },
        new SelectListItem { Value = "Master", Text = "Master" },
        new SelectListItem { Value = "PhD", Text = "PhD" }
    };

            // Initialize EnglishProficiencyLevels before inserting
            model.EnglishProficiencyLevels = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "Select English Proficiency" },
        new SelectListItem { Value = "Beginner", Text = "Beginner" },
        new SelectListItem { Value = "Intermediate", Text = "Intermediate" },
        new SelectListItem { Value = "Advanced", Text = "Advanced" },
        new SelectListItem { Value = "Fluent", Text = "Fluent" }
    };
            model.EnglishProficiencyLevels.Insert(1, new SelectListItem { Value = "Other", Text = "Other" });
        }




    }
}

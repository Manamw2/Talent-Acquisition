using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Models;
using Models.Mappers;
using Models.ViewModels;
using System.Net.Http;
using System.Text.Json;

namespace HrBackOffice.Helper
{
    public  class CVSetting
    {
        private readonly HttpClient _httpClient;
        public CVSetting(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

     
        public async Task<ProfileInfoVM> ExtractDataFromCv(IFormFile cvFile)
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

                // Make the API call
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/parse-resume", content);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                ResumeViewModel resumeViewModel = JsonSerializer.Deserialize<ResumeViewModel>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var profileVM = resumeViewModel.ResumeToProfileInfo();
                return profileVM;
            }
        }
    }
}

using HrBackOffice.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HrBackOffice.Helper.ApplicantService
{
    public interface IApplicantService
    {

        public Task<UserViewModel> ExtractDataFromCv(IFormFile cvFile);

        public void PopulateDropdownLists(UserViewModel model);
        
    }
}

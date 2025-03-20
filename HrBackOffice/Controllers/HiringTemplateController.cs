using Microsoft.AspNetCore.Mvc;

namespace HrBackOffice.Controllers
{
    public class HiringTemplateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

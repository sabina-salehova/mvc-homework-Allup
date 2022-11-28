using Microsoft.AspNetCore.Mvc;

namespace Allup.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

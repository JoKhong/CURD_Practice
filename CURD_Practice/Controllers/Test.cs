using Microsoft.AspNetCore.Mvc;

namespace CURD_Practice.Controllers
{
    public class Test : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

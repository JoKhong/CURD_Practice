using Microsoft.AspNetCore.Mvc;

namespace CURD_Practice.Controllers
{
    public class PersonsController : Controller
    {
        [Route("persons/index")]
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}

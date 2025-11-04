using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CURD_Practice.Controllers
{
    public class PersonsController : Controller
    {
        private readonly IPersonsServices _personsServices;

        public PersonsController(IPersonsServices personsServices)
        {
            _personsServices = personsServices;
        }

        [Route("persons/index")]
        [Route("/")]
        public IActionResult Index()
        {
            List<PersonResponse> allPersons = _personsServices.GetAllPersons();

            return View(allPersons);
        }
    }
}

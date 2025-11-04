using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CURD_Practice.Controllers
{
    [Route("[controller]")]

    public class PersonsController : Controller
    {
        private readonly IPersonsServices _personsServices;
        private readonly ICountriesService _countratesServices;

        public PersonsController(IPersonsServices personsServices, ICountriesService countratesServices)
        {
            _personsServices = personsServices;
            _countratesServices = countratesServices;
        }

        [Route("[action]")]
        [Route("/")]
        public IActionResult Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            //Searching
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                { nameof(PersonResponse.PersonName), "Person Name"},
                { nameof(PersonResponse.Email), "Email"},
                { nameof(PersonResponse.DateOfBirth), "Date of Birth"},
                { nameof(PersonResponse.Age), "Age"},
                { nameof(PersonResponse.Gender), "Gender"},
                { nameof(PersonResponse.Country), "Country"},
                { nameof(PersonResponse.Address), "Address"},
            };

            List <PersonResponse> responsePersons = _personsServices.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sorting
            List<PersonResponse> sortedPersons = _personsServices.GetSortedPersons(responsePersons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedPersons);
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> selectCountires = _countratesServices.GetAllCountries().
                Select(aCountry => new SelectListItem() 
                { 
                    Text = aCountry.CountryName, 
                    Value = aCountry.CountryId.ToString() 
                });

            ViewBag.Countries = selectCountires;

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public IActionResult Create(PersonAddRequest addRequest)
        {
            if (!ModelState.IsValid) {

                ViewBag.Countries = _countratesServices.GetAllCountries();
                ViewBag.Errors = ModelState.Values.SelectMany( v => v.Errors ).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            _personsServices.AddPerson(addRequest);

            return RedirectToAction("Index", "Persons");
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public IActionResult Edit(Guid personId)
        {
            PersonResponse personById = _personsServices.GetPersonById(personId);

            if(personById == null)
                return RedirectToAction("Index", "Persons");

            PersonUpdateRequest updateRequest = personById.ToPersonUpdateRequest();

            IEnumerable<SelectListItem> selectCountires = _countratesServices.GetAllCountries().
                Select(aCountry => new SelectListItem()
                {
                    Text = aCountry.CountryName,
                    Value = aCountry.CountryId.ToString()
                });

            ViewBag.Countries = selectCountires;

            return View(updateRequest);
        }

        [Route("[action]")]
        [HttpPost]
        public IActionResult Edit(PersonUpdateRequest upDateRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = _countratesServices.GetAllCountries();
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            _personsServices.UpdatePerson(upDateRequest);

            return RedirectToAction("Index", "Persons");
        }

    }
}

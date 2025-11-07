using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using Microsoft.Data.SqlClient;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System.Globalization;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
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

            List <PersonResponse> responsePersons = await _personsServices.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sorting
            List<PersonResponse> sortedPersons = await _personsServices.GetSortedPersons(responsePersons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedPersons);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            List<CountryResponse> allCountires = await _countratesServices.GetAllCountries();

            IEnumerable <SelectListItem> selectCountires =
                allCountires.Select(aCountry => new SelectListItem() 
                { 
                    Text = aCountry.CountryName, 
                    Value = aCountry.CountryId.ToString() 
                });

            ViewBag.Countries = selectCountires;

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(PersonAddRequest addRequest)
        {
            if (!ModelState.IsValid) {

                ViewBag.Countries = _countratesServices.GetAllCountries();
                ViewBag.Errors = ModelState.Values.SelectMany( v => v.Errors ).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            await _personsServices.AddPerson(addRequest);

            return RedirectToAction("Index", "Persons");
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid personId)
        {
            PersonResponse? personById = await _personsServices.GetPersonById(personId);

            if(personById == null)
                return RedirectToAction("Index", "Persons");

            PersonUpdateRequest updateRequest = personById.ToPersonUpdateRequest();

            List<CountryResponse> allCountries = await _countratesServices.GetAllCountries();

            IEnumerable <SelectListItem> selectCountires =
                allCountries.Select(aCountry => new SelectListItem()
                {
                    Text = aCountry.CountryName,
                    Value = aCountry.CountryId.ToString()
                });

            ViewBag.Countries = selectCountires;

            return View(updateRequest);
        }

        [Route("[action]/{personId}")]
        [HttpPost]
        public async Task<IActionResult> Edit(PersonUpdateRequest upDateRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = _countratesServices.GetAllCountries();
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            await _personsServices.UpdatePerson(upDateRequest);

            return RedirectToAction("Index", "Persons");
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid personId)
        {
            PersonResponse? personById = await _personsServices.GetPersonById(personId);

            if (personById == null)
                return RedirectToAction("Index", "Persons");

            PersonUpdateRequest updateRequest = personById.ToPersonUpdateRequest();

            List<CountryResponse> allCountries = await _countratesServices.GetAllCountries();

            IEnumerable <SelectListItem> selectCountires = 
                allCountries.Select(aCountry => new SelectListItem()
                {
                    Text = aCountry.CountryName,
                    Value = aCountry.CountryId.ToString()
                });

            ViewBag.Countries = selectCountires;

            return View(updateRequest);
        }

        [Route("[action]/{personId}")]
        [HttpPost]
        public async Task<IActionResult> Delete(PersonUpdateRequest upDateRequest)
        {
            PersonResponse? personResponse = await _personsServices.GetPersonById(upDateRequest.PersonId);
            if (personResponse == null)
                return RedirectToAction("Index", "Persons");

            await _personsServices.DeletePerson(upDateRequest.PersonId);
            return RedirectToAction("Index", "Persons");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsPdf() 
        {
            List<PersonResponse> responsePersons = await _personsServices.GetAllPersons();

            ViewAsPdf viewAsPdf = new ViewAsPdf("PersonsPdf", responsePersons, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 20, 20, 20),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };

            return viewAsPdf;

        }

    }



}

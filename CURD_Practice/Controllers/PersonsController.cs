using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using Microsoft.Data.SqlClient;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System.Globalization;
using System.Threading.Tasks;
using CURD_Practice.Filters.ActionFilters;

namespace CURD_Practice.Controllers
{
    //TypeFilters Orders follow a = Last in, First out
    //Execute from lowest to highest

    [Route("[controller]")]
    [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Controller-Key", "X-Controller-Value" })]
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Controller-Key", "X-Controller-Value" , 2}, Order = 2)] //Force Set order 
    public class PersonsController : Controller
    {
        private readonly IPersonsServices _personsServices;
        private readonly ICountriesService _countratesServices;

        private readonly ILogger<PersonsController> _logger;

        public PersonsController(IPersonsServices personsServices, ICountriesService countratesServices, ILogger<PersonsController> logger)
        {
            _personsServices = personsServices;
            _countratesServices = countratesServices;
            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
        [TypeFilter(typeof(PersonsListActionFilter))]
        [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Action-Key", "X-Action-Value"})]
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Action-Key", "X-Action-Value" , 1} , Order = 1)] // Force Set order 
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("Index method of PersonsController entered");

            _logger.LogDebug($"searchby:{searchBy}, " +
                $"searchString:{searchString}, " +
                $"sortBy:{sortBy}, " +
                $"sortOrder:{sortOrder}"
                );

            //Searching

            /* Done in ActionFilters
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
            */

            List <PersonResponse> responsePersons = await _personsServices.GetFilteredPersons(searchBy, searchString);//Filter 
            List<PersonResponse> sortedPersons = await _personsServices.GetSortedPersons(responsePersons, sortBy, sortOrder);//Sort
            
            /* Done in ActionFilter
                        ViewBag.CurrentSearchBy = searchBy;
                        ViewBag.CurrentSearchString = searchString;
                        ViewBag.CurrentSortBy = sortBy;
                        ViewBag.CurrentSortOrder = sortOrder.ToString();
            */

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
                return View(addRequest);
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

        [Route("[action]")]
        public async Task<IActionResult> PersonsCsv()
        {
            MemoryStream stream =  await _personsServices.GetPersonsCSV();
            return File(stream, "application/octet-stream", "persons.csv");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsCsvCustom()
        {
            MemoryStream stream = await _personsServices.GetPersonsCSVCustom();
            return File(stream, "application/octet-stream", "persons.csv");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream stream = await _personsServices.GetPersonsExcel();
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }

    }



}

using CURD_Practice.Filters;
using CURD_Practice.Filters.ActionFilters;
using CURD_Practice.Filters.AlwaysRunResultFilter;
using CURD_Practice.Filters.AuthorizationFilters;
using CURD_Practice.Filters.ExceptionFilters;
using CURD_Practice.Filters.ResourceFilters;
using CURD_Practice.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Globalization;
using System.Threading.Tasks;

namespace CURD_Practice.Controllers
{
    //TypeFilters Orders follow a = Last in, First out
    //Execute from lowest to highest

    [Route("[controller]")]
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Controller-Key", "X-Controller-Value" })]
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Controller-Key", "X-Controller-Value" , 1}, Order = 1)] //Force Set order 
    [ResponseHeaderFilterFactoryAttribute("X-Controller-Key", "X-Controller-Value", 1)]
    //[TypeFilter(typeof(HandleExceptionFilter))]
    //[TypeFilter(typeof(PersonsAlwaysRunsResultFilter))]
    public class PersonsController : Controller
    {
        //private readonly IPersonsServices _personsServices;

        private readonly IPersonAdderService _personAdderService;
        private readonly IPersonGetterServices _personGetPersonsServices;
        private readonly IPersonSortPersonsService _personGetSortedPersonsServices;
        private readonly IPersonUpdatePersonService _personUpdatePersonService;
        private readonly IPersonDeletePersonService _personDeletePersonService;

        private readonly IPersonsToCSVService _personGetPersonsCSVService;
        private readonly IPersonsToExcelService _personGetPersonsExcelService;

        private readonly ICountryGetterServices _countryGetCountriesServices;

        private readonly ILogger<PersonsController> _logger;

        public PersonsController(IPersonAdderService personAdderService
            , IPersonGetterServices personGetPersonsServices
            , IPersonSortPersonsService personGetSortedPersonsService
            , IPersonUpdatePersonService personUpdatePersonService
            , IPersonDeletePersonService personDeletePersonService
            , IPersonsToCSVService personGetPersonsCSVService
            , IPersonsToExcelService personGetPersonsExcelService
            , ICountryGetterServices countryGetCountriesServices
            , ILogger<PersonsController> logger)
        {
            _personAdderService = personAdderService;
            _personGetPersonsServices = personGetPersonsServices;
            _personGetSortedPersonsServices = personGetSortedPersonsService;
            _personUpdatePersonService = personUpdatePersonService;
            _personDeletePersonService = personDeletePersonService;

            _personGetPersonsCSVService = personGetPersonsCSVService;
            _personGetPersonsExcelService = personGetPersonsExcelService;

            _countryGetCountriesServices = countryGetCountriesServices;

            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
        [TypeFilter(typeof(PersonsListActionFilter))]
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Action-Key", "X-Action-Value"})]
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Action-Key", "X-Action-Value" , 2} , Order = 2)] // Force Set order 
        [ResponseHeaderFilterFactoryAttribute("X-Action-Key", "X-Action-Value", 2)]
        [TypeFilter(typeof(PersonsListResultFilter))]
        [SkipFilter]
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

            List<PersonResponse> responsePersons = await _personGetPersonsServices.GetFilteredPersons(searchBy, searchString);//Filter 
            List<PersonResponse> sortedPersons = await _personGetSortedPersonsServices.GetSortedPersons(responsePersons, sortBy, sortOrder);//Sort
            
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
            List<CountryResponse> allCountires = await _countryGetCountriesServices.GetCountriesAll();

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
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        //[TypeFilter(typeof(FeatureDisableResourceFilter), Arguments = new object[] { true })]
        public async Task<IActionResult> Create(PersonAddRequest personRequest)
        {  
            if(ModelState.IsValid == false)
                return View(personRequest);

            await _personAdderService.AddPerson(personRequest);
                return RedirectToAction("Index", "Persons");
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        [TypeFilter(typeof(TokenResultFilter))]//Add cookie during get
        public async Task<IActionResult> Edit(Guid personId)
        {
            PersonResponse? personById = await _personGetPersonsServices.GetPersonById(personId);

            if(personById == null)
                return RedirectToAction("Index", "Persons");

            PersonUpdateRequest updateRequest = personById.ToPersonUpdateRequest();

            List<CountryResponse> allCountries = await _countryGetCountriesServices.GetCountriesAll();

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
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(TokenAuthorizationFileter))]//Authorize update
        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
        {
            PersonResponse? response = await _personGetPersonsServices.GetPersonById(personRequest.PersonId);

            if (response == null)
                return RedirectToAction("Index");

            await _personUpdatePersonService.UpdatePerson(personRequest);
            return RedirectToAction("Index", "Persons");
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid personId)
        {
            PersonResponse? personById = await _personGetPersonsServices.GetPersonById(personId);

            if (personById == null)
                return RedirectToAction("Index", "Persons");

            PersonUpdateRequest updateRequest = personById.ToPersonUpdateRequest();

            List<CountryResponse> allCountries = await _countryGetCountriesServices.GetCountriesAll();

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
        public async Task<IActionResult> Delete(PersonUpdateRequest deleteRequest)
        {
            PersonResponse? personResponse = await _personGetPersonsServices.GetPersonById(deleteRequest.PersonId);
            if (personResponse == null)
                return RedirectToAction("Index", "Persons");

            await _personDeletePersonService.DeletePerson(deleteRequest.PersonId);
            return RedirectToAction("Index", "Persons");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsPdf() 
        {
            List<PersonResponse> responsePersons = await _personGetPersonsServices.GetAllPersons();

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
            MemoryStream stream =  await _personGetPersonsCSVService.PersonsToSCV(await _personGetPersonsServices.GetAllPersons());
            return File(stream, "application/octet-stream", "persons.csv");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream stream = await _personGetPersonsExcelService.PersonsToExcel(await _personGetPersonsServices.GetAllPersons());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }

    }



}

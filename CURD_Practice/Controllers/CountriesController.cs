using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using Services;

namespace CURD_Practice.Controllers
{
    [Route("[controller]")]

    public class CountriesController : Controller
    {
        //private readonly ICountriesService _countratesServices;
        //private readonly ICountryGetCountriesServices _countryGetCountriesServices;
        private readonly ICountryUploadFromExcelService _countriesUploadFromExcelService;

        public CountriesController(ICountryUploadFromExcelService countriesUploadFromExcelService)
        {
            //_countratesServices = countratesServices;
            //_countryGetCountriesServices = countryGetCountriesServices;
            _countriesUploadFromExcelService = countriesUploadFromExcelService;
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult UploadFromExcel()
        {
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0) 
            {
                ViewBag.ErrorMessage = "Please select an xlsx file";
                return View();
            }

            if(!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "File is not xlsx file";
                return View();
            }

            int countriesInserted =  await _countriesUploadFromExcelService.UploadCountiresFromExcelFile(excelFile);

            ViewBag.Message = $"{countriesInserted} Countries added";

            return View();
        }
    }

 
}

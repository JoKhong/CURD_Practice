using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using Services;

namespace CURD_Practice.Controllers
{
    [Route("[controller]")]

    public class CountriesController : Controller
    {
        private readonly ICountriesService _countratesServices;

        public CountriesController(ICountriesService countratesServices)
        {
            _countratesServices = countratesServices;
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

            int countriesInserted =  await _countratesServices.UploadCountiresFromExcelFile(excelFile);

            ViewBag.Message = $"{countriesInserted} Countries added";

            return View();
        }
    }

 
}

using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using System.Diagnostics.Metrics;

namespace Services
{
    public class CountryUploadFromExcelService : ICountryUploadFromExcelService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountryUploadFromExcelService(ICountriesRepository countriesRepo)
        {
            _countriesRepository = countriesRepo;
        }

        public async Task<int> UploadCountiresFromExcelFile(IFormFile formFile)
        {
            int countriesInserted = 0;

            MemoryStream stream = new MemoryStream();

            await formFile.CopyToAsync(stream);

            using (ExcelPackage excelPackage = new ExcelPackage(stream))
            {
                ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets["Countires"];

                int rows = workSheet.Dimension.Rows;
               

                for (int i = 2; i <= rows; i++) 
                {
                    string? cellValue = Convert.ToString(workSheet.Cells[i, 1].Value);

                    if(!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;

                        if(_countriesRepository.GetCountryByName(countryName) is null)
                        {
                            Country addRequest = new Country() { CountryName = countryName };
                            await _countriesRepository.AddCountry(addRequest);

                            countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}

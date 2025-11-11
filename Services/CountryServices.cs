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
    public class CountryServices : ICountriesService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountryServices(ICountriesRepository countriesRepo)
        {
            _countriesRepository = countriesRepo;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? AddRequest)
        {
            if (AddRequest == null)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(AddRequest.CountryName))
            {
                throw new ArgumentException("AddRequest.CountryName is null or empty");
            }

            if (await _countriesRepository.GetCountryByName(AddRequest.CountryName) is not null)
            {
                throw new ArgumentException("Country already added");
            }

            try 
            {
                Country country = AddRequest.ToCountry();
                country.CountryId = Guid.NewGuid();

                await _countriesRepository.AddCountry(country);

                return country.ToCountryResponse();
            }
            catch(Exception ex)
            {
                return null;
            }

        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            List<Country> countries = await _countriesRepository.GetAllCountries();

            return countries.Select(temp => temp.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryById(Guid? id)
        {
            if(id == null)
                return null;

            Country? validCountry = await _countriesRepository.GetCountryById(id.Value);

            if (validCountry == null)
                return null;

            return  validCountry.ToCountryResponse();
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

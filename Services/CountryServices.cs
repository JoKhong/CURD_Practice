using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;
using System.Diagnostics.Metrics;

namespace Services
{
    public class CountryServices : ICountriesService
    {
        private readonly ApplicationDbContext _db;

        public CountryServices(ApplicationDbContext personsDbContext)
        {
            _db = personsDbContext;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? AddRequest)
        {
            if (AddRequest == null)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(AddRequest.CountryName))
            {
                throw new ArgumentException("AddRequest.CountryName is null or empty");
            }

            if (await _db.Countries.CountAsync(aCountry => aCountry.CountryName == AddRequest.CountryName) > 0)
            {
                throw new ArgumentException("Country already added");
            }

            try 
            {
                Country country = AddRequest.ToCountry();
                country.CountryId = Guid.NewGuid();

                _db.Countries.Add(country);
                await _db.SaveChangesAsync();

                return country.ToCountryResponse();
            }
            catch(Exception ex)
            {
                return null;
            }

        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return await _db.Countries.Select(country => country.ToCountryResponse()).ToListAsync();
        }

        public async Task<CountryResponse?> GetCountryById(Guid? id)
        {
            if(id == null)
                return null;

            Country? validCountry = await _db.Countries.Where( country => country.CountryId == id).FirstAsync();

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

                        if(_db.Countries.Where( country => country.CountryName == countryName ).Count() == 0)
                        {
                            Country addRequest = new Country() { CountryName = countryName };
                            _db.Countries.Add(addRequest);

                            await _db.SaveChangesAsync();
                            countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}

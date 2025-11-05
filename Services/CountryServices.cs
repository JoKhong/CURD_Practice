using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountryServices : ICountriesService
    {
        private readonly PersonsDbContext _db;

        public CountryServices(PersonsDbContext personsDbContext)
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
    }
}

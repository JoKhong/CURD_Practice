using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repositories
{
    public class CountriesRepositories : ICountriesRepository
    {
        private readonly ApplicationDbContext _db;

        public CountriesRepositories(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Country> AddCountry(Country country)
        {
            _db.Add(country);
            await _db.SaveChangesAsync();

            return country;
        }

        public async Task<List<Country>> GetAllCountries()
        {
            return await _db.Countries.ToListAsync();
        }

        public async Task<Country?> GetCountryById(Guid countryId)
        {
            return await _db.Countries.FirstOrDefaultAsync(temp => temp.CountryId == countryId);
        }

        public async Task<Country?> GetCountryByName(string name)
        {
            return await _db.Countries.FirstOrDefaultAsync(temp => temp.CountryName == name);
        }
    }
}

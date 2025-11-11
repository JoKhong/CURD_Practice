using Entities;

namespace RepositoryContracts
{
    /// <summary>
    /// Data access logic for managing Countires
    /// </summary>
    public interface ICountriesRepository
    {
        Task<Country> AddCountry(Country country);
        Task<List<Country>> GetAllCountries();
        Task<Country?> GetCountryById(Guid countryId);
        Task<Country?> GetCountryByName(string name);
    }
}

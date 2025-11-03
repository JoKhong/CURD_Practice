using ServiceContracts.DTO;
using Entities;

namespace ServiceContracts
{
    /// <summary>
    /// Adds a country object to the list of countries
    /// </summary>
    /// <param name="AddRequest">Country object to add</param>
    /// <returns>Returns the country object after adding it (including newly generated country id)</returns>
    public interface ICountriesService
    {
        CountryResponse AddCountry(CountryAddRequest? AddRequest);

        /// <summary>
        /// Returns All countires
        /// </summary>
        /// <returns>Returns All Countires</returns>
        List<CountryResponse> GetAllCountries();
    }
}

using ServiceContracts.DTO;
using Entities;

namespace ServiceContracts
{
    /// <summary>
    /// Business logic for manipulating Country Entity
    /// </summary>
    public interface ICountriesService
    {
        CountryResponse AddCountry(CountryAddRequest? AddRequest);
    }
}

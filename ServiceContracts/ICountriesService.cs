using ServiceContracts.DTO;
using Entities;
using ServiceContracts.Enums;

using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;


namespace ServiceContracts
{
    /// <summary>
    /// Adds a country object to the list of countries
    /// </summary>
    /// <param name="AddRequest">Country object to add</param>
    /// <returns>Returns the country object after adding it (including newly generated country id)</returns>
    public interface ICountriesService
    {
        Task<CountryResponse> AddCountry(CountryAddRequest? AddRequest);

        /// <summary>
        /// Returns All countires
        /// </summary>
        /// <returns>Returns All Countires</returns>
        Task<List<CountryResponse>>  GetAllCountries();

        /// <summary>
        /// Return country object based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Matching country object</returns>
        Task<CountryResponse?> GetCountryById(Guid? id);

        Task <int> UploadCountiresFromExcelFile(IFormFile formFile);

    }
}

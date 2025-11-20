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
    public interface ICountryGetterServices
    {
        Task<List<CountryResponse>>  GetCountriesAll();
        Task<CountryResponse?> GetCountryById(Guid? id);
    }
}

using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Formats.Asn1;
using System.Globalization;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonGetterServices
    {  
        /// <summary>
        /// Gets all Persons 
        /// </summary>
        /// <returns></returns>
        Task<List<PersonResponse>> GetAllPersons();
        Task<PersonResponse?> GetPersonById(Guid? id);
        
        /// <summary>
        /// Get Persons based on search by field and Search name
        /// </summary>
        /// <param name="searchBy"></param>
        /// <param name="searchString"></param>
        /// <returns>Returns PersonResponse based on searchBy and searchString</returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);

        Task<MemoryStream> GetPersonsCSV(List<PersonResponse> AllPersons);
        Task<MemoryStream> GetPersonsExcel(List<PersonResponse> allPersons);

    }
}

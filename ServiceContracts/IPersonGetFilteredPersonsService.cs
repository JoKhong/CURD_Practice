using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonGetFilteredPersonsService
    {
        /// <summary>
        /// Get Persons based on search by field and Search name
        /// </summary>
        /// <param name="searchBy"></param>
        /// <param name="searchString"></param>
        /// <returns>Returns PersonResponse based on searchBy and searchString</returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);
    }
}

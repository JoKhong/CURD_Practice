using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonsServices
    {
        /// <summary>
        /// Add a new Person into existing list of Persons
        /// </summary>
        /// <param name="addRequest"></param>
        /// <returns></returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? addRequest);

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

        /// <summary>
        /// Returns sorted list of persons
        /// </summary>
        /// <param name="allPersons"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns>Returns sorted list of persons</returns>
        Task<List<PersonResponse>>  GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder);

        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? updateRequest);

        Task<bool> DeletePerson(Guid? PersonId);

        Task<MemoryStream> GetPersonsCSV();

        Task<MemoryStream> GetPersonsCSVCustom();

    }
}

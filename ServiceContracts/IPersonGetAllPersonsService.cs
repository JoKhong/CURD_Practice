using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonGetAllPersonsService
    {
        /// <summary>
        /// Gets all Persons 
        /// </summary>
        /// <returns></returns>
        Task<List<PersonResponse>> GetAllPersons();
    }
}

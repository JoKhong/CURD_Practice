using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonAdderService
    {
        /// <summary>
        /// Add a new Person into existing list of Persons
        /// </summary>
        /// <param name="addRequest"></param>
        /// <returns></returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? addRequest);
    }
}

using System;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    public interface IPersonsServices
    {
        /// <summary>
        /// Add a new Person into existing list of Persons
        /// </summary>
        /// <param name="addRequest"></param>
        /// <returns></returns>
        PersonResponse AddPerson(PersonAddRequest? addRequest);

        /// <summary>
        /// Gets all Persons 
        /// </summary>
        /// <returns></returns>
        List<PersonResponse> GetAllPersons();

        PersonResponse? GetPersonById(Guid? id);

    }
}

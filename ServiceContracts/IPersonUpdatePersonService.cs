using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonUpdatePersonService
    {
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? updateRequest);
    }
}

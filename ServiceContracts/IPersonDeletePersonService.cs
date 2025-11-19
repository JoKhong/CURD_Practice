using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonDeletePersonService
    {
        Task<bool> DeletePerson(Guid? PersonId);
    }
}

using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonGetByIdService
    {
        Task<PersonResponse?> GetPersonById(Guid? id);
    }
}

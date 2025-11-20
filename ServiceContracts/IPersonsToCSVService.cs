using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonsToCSVService
    {
        Task<MemoryStream> PersonsToSCV(List<PersonResponse> AllPersons);
    }
}

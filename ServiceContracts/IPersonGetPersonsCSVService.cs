using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonGetPersonsCSVService
    {
        Task<MemoryStream> GetPersonsCSV(List<PersonResponse> AllPersons);
    }
}

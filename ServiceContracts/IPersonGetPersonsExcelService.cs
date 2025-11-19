using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonGetPersonsExcelService
    {
        Task<MemoryStream> GetPersonsExcel(List<PersonResponse> allPersons);
    }
}

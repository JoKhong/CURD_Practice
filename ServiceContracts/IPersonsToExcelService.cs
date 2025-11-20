using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonsToExcelService
    {
        Task<MemoryStream> PersonsToExcel(List<PersonResponse> allPersons);
    }
}

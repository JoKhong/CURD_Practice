using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.ExternalReferences;
using RepositoryContracts;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System.Formats.Asn1;
using System.Globalization;

using SerilogTimings;
using Exceptions;

namespace Services
{
    public class PersonToExcelService : IPersonsToExcelService
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonToExcelService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;

        public PersonToExcelService(IPersonsRepository personRepo, ILogger<PersonToExcelService> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;
            _logger = logger;

            _diagnosticContext = diagContext;
        }

        public async Task<MemoryStream> PersonsToExcel(List<PersonResponse> allPersons)
        {
            _logger.LogInformation("GetPersonsExcel of PersonServices");

            MemoryStream stream = new MemoryStream();

            using var package = new ExcelPackage();
            
            using (ExcelPackage excelPackage = new ExcelPackage(stream)) 
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");
                worksheet.Cells["A1"].Value = nameof(PersonResponse.PersonName);
                worksheet.Cells["B1"].Value = (nameof(PersonResponse.Email));
                worksheet.Cells["C1"].Value = (nameof(PersonResponse.DateOfBirth));
                worksheet.Cells["D1"].Value = (nameof(PersonResponse.Age));
                worksheet.Cells["E1"].Value = (nameof(PersonResponse.Gender));
                worksheet.Cells["F1"].Value = (nameof(PersonResponse.Country));
                worksheet.Cells["G1"].Value = (nameof(PersonResponse.Address));
                worksheet.Cells["H1"].Value = (nameof(PersonResponse.ReceiveNewsLetters));

                int row = 2;
                List<PersonResponse> response = allPersons;

                foreach(PersonResponse person in response)
                {
                    worksheet.Cells[row, 1].Value = person.PersonName;
                    worksheet.Cells[row, 2].Value = person.Email;

                    if (person.DateOfBirth.HasValue)
                        worksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("yyyy-MM-dd");
                    else
                        worksheet.Cells[row, 3].Value = "";

                    worksheet.Cells[row, 4].Value = person.Age;
                    worksheet.Cells[row, 5].Value = person.Gender;
                    worksheet.Cells[row, 6].Value = person.Country;
                    worksheet.Cells[row, 7].Value = person.Address;
                    worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;

                    row++;
                }

                worksheet.Cells[$"A1:H{row}"].AutoFitColumns();

                await excelPackage.SaveAsync();

            }

            stream.Position = 0;
            return stream;

        }
    }
    
}

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
    public class PersonToCSVServiceCustom : IPersonsToCSVService
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonToCSVServiceCustom> _logger;

        private readonly IDiagnosticContext _diagnosticContext;


        public PersonToCSVServiceCustom(IPersonsRepository personRepo, ILogger<PersonToCSVServiceCustom> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;
            _logger = logger;

            _diagnosticContext = diagContext;
        }

        public async Task<MemoryStream> PersonsToSCV(List<PersonResponse> AllPersons)
        {
            _logger.LogInformation("GetPersonsCSVCustom of PersonServices");

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            //Get Data
            List<PersonResponse> response = AllPersons;

            CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvWriter csvWriter = new CsvWriter(writer, csvConfig);

            //PrepareHeader
            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Country));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
            csvWriter.NextRecord();

            //Fill in Data
            foreach (PersonResponse person in response) 
            {
                csvWriter.WriteField(person.PersonName);
                csvWriter.WriteField(person.Email);

                if(person.DateOfBirth.HasValue)
                    csvWriter.WriteField(person.DateOfBirth.Value.ToString("yyyy-MM-dd"));
                else
                    csvWriter.WriteField("");

                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.Gender);
                csvWriter.WriteField(person.Country);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.ReceiveNewsLetters);

                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            stream.Position = 0;
            return stream;
        }

    }
    
}

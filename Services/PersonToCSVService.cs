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
    public class PersonToCSVService : IPersonsToCSVService
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonToCSVService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;

        public PersonToCSVService(IPersonsRepository personRepo, ILogger<PersonToCSVService> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;

            _logger = logger;

            _diagnosticContext = diagContext;
        }

        public async Task<MemoryStream> PersonsToSCV(List<PersonResponse> AllPersons)
        {
            _logger.LogInformation("GetPersonsCSV of PersonServices");

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            //Get Data
            List<PersonResponse> response = AllPersons;

            CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture , leaveOpen:true);

            
            //Prepare Header
            csvWriter.WriteHeader<PersonResponse>();
            csvWriter.NextRecord();

            //Fill in Data
            await csvWriter.WriteRecordsAsync(response);

            stream.Position = 0;
            return stream;

        }

    }
    
}

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
    public class PersonGetAllPersonsService : IPersonGetAllPersonsService
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonGetAllPersonsService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;

        public PersonGetAllPersonsService(IPersonsRepository personRepo, ILogger<PersonGetAllPersonsService> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;
            _logger = logger;

            _diagnosticContext = diagContext;
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonServices");

            var persons = await _personRepository.GetAllPersons();

            return persons.Select(temp => temp.ToPersonResponse() ).ToList();
        }

    }
    
}

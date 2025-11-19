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
    public class PersonGetByIdService : IPersonGetByIdService
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonGetByIdService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;

        public PersonGetByIdService(IPersonsRepository personRepo, ILogger<PersonGetByIdService> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;
            _logger = logger;

            _diagnosticContext = diagContext;
        }

        public async Task<PersonResponse?> GetPersonById(Guid? id)
        {
            _logger.LogInformation("GetPersonById of PersonServices");
            //throw new NotImplementedException();

            if (id == null)
                return null;

            Person? response = await _personRepository.GetPersonById(id.Value);

            if (response == null)
                return null;

            return response.ToPersonResponse();
        }

    }
    
}

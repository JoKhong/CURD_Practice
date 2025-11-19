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
    public class PersonDeletePersonService : IPersonDeletePersonService
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonDeletePersonService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;

        public PersonDeletePersonService(IPersonsRepository personRepo, ILogger<PersonDeletePersonService> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;
            _logger = logger;

            _diagnosticContext = diagContext;
        }

        public async Task<bool> DeletePerson(Guid? personId)
        {
            _logger.LogInformation("DeletePerson of PersonServices");

            if (personId == null || personId == Guid.Empty)
                return false;

            Person? matchingPerson = await _personRepository.GetPersonById(personId.Value);

            if (matchingPerson == null)
                return false;

            await _personRepository.DeletePersonsById(matchingPerson.PersonId);

            return true;
        }

    }
    
}

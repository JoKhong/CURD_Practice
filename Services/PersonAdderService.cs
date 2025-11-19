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
    public class PersonAdderService : IPersonAdderService
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonAdderService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;

        public PersonAdderService(IPersonsRepository personRepo, ILogger<PersonAdderService> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;
            _logger = logger;

            _diagnosticContext = diagContext;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? addRequest)
        {
            _logger.LogInformation("AddPerson of PersonServices");

            if (addRequest == null)
                throw new ArgumentNullException();

            //Model Validation
            ValidationHelper.ModelValidation(addRequest);

            //List<PersonResponse> results =  (await _personRepository.GetAllPersons())
            //    .Select(temp => temp.ToPersonResponse()).ToList();

            //if (results.FirstOrDefault( temp => temp.PersonName == addRequest.PersonName ) is not null)
            //    throw new ArgumentException("Duplicate Names");

            try 
            {
                Person person = addRequest.ToPerson();
                person.PersonId = Guid.NewGuid();

                await _personRepository.AddPerson(person);

                return person.ToPersonResponse();
            }
            catch(Exception ex)
            {
                if (addRequest == null)
                    throw new ArgumentNullException();

                return null;
            }
        }

    }
    
}

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
    public class PersonUpdatePersonService : IPersonUpdatePersonService
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonUpdatePersonService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;

        public PersonUpdatePersonService(IPersonsRepository personRepo, ILogger<PersonUpdatePersonService> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;
            _logger = logger;

            _diagnosticContext = diagContext;
        }
        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? updateRequest)
        {
            _logger.LogInformation("UpdatePerson of PersonServices");

            if (updateRequest == null)
                throw new ArgumentNullException(nameof(updateRequest));

            ValidationHelper.ModelValidation(updateRequest);

            Person? matchingPerson = await _personRepository.GetPersonById(updateRequest.PersonId);

            if (matchingPerson == null)
                throw new InvalidPersonIdExceptions("Given Id dose not exist");

            try 
            {
                matchingPerson.PersonName = updateRequest.PersonName;
                matchingPerson.Email = updateRequest.Email;
                matchingPerson.DateOfBirth = updateRequest.DateOfBirth;
                matchingPerson.Gender = updateRequest.Gender.ToString();
                matchingPerson.CountryId = updateRequest.CountryId;
                matchingPerson.Address = updateRequest.Address;
                matchingPerson.ReceiveNewsLetters = updateRequest.ReceiveNewsLetters;

                await _personRepository.UpdatePerson(matchingPerson);
                return matchingPerson.ToPersonResponse();
            }
            catch (Exception ex) 
            {
                return null;
            }
        }
    }
    
}

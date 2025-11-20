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
    public class PersonGetterServices : IPersonGetterServices
    {
        private readonly IPersonsRepository _personRepository;

        private readonly ILogger<PersonGetterServices> _logger;

        private readonly IDiagnosticContext _diagnosticContext;

        private readonly IPersonsToCSVService _personToCSVService;
        private readonly IPersonsToExcelService _personToExcelService;

        public PersonGetterServices(IPersonsRepository personRepo, IPersonsToCSVService personToCSVService, IPersonsToExcelService personsToExcelService, ILogger<PersonGetterServices> logger, IDiagnosticContext diagContext)
        {
            _personRepository = personRepo;
            _logger = logger;

            _personToCSVService = personToCSVService;
            _personToExcelService = personsToExcelService;

            _diagnosticContext = diagContext;
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonServices");

            var persons = await _personRepository.GetAllPersons();

            return persons.Select(temp => temp.ToPersonResponse() ).ToList();
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

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            _logger.LogInformation("GetFilteredPersons of PersonServices");
            //_logger.LogDebug($"searchBy:{searchBy}, searchString:{searchString}");

            List<Person>? persons = null as List<Person>;

            using (Operation.Time("Filter from Database"))
            {
                persons = searchBy switch
                {
                    nameof(PersonResponse.PersonName) =>
                        await _personRepository.GetFilteredPersons(temp =>
                        temp.PersonName.Contains(searchString)),

                    nameof(PersonResponse.Email) =>
                        await _personRepository.GetFilteredPersons(temp =>
                        temp.Email.Contains(searchString)),

                    nameof(PersonResponse.Gender) =>
                        await _personRepository.GetFilteredPersons(temp =>
                        temp.Gender.ToLower() == searchString.ToLower()),

                    nameof(PersonResponse.CountryId) =>
                        await _personRepository.GetFilteredPersons(temp =>
                        temp.Country.CountryName.Contains(searchString)),

                    nameof(PersonResponse.Country) =>
                        await _personRepository.GetFilteredPersons(temp =>
                        temp.Country.CountryName.Contains(searchString)),

                    nameof(PersonResponse.Address) =>
                        await _personRepository.GetFilteredPersons(temp =>
                        temp.Address.Contains(searchString)),

                    nameof(PersonResponse.DateOfBirth) =>
                        await _personRepository.GetFilteredPersons(temp =>
                        temp.DateOfBirth.Value.ToString("dd MMMM yy").Contains(searchString)),

                    _ => await _personRepository.GetAllPersons(),
                };

            }

            _diagnosticContext.Set("Persons", persons);

            return persons.Select(temp => temp.ToPersonResponse()).ToList();

        }

        public async Task<MemoryStream> GetPersonsCSV(List<PersonResponse> AllPersons)
        {
            return await _personToCSVService.PersonsToSCV(AllPersons);
        }

        public async Task<MemoryStream> GetPersonsExcel(List<PersonResponse> allPersons)
        {
            return await _personToExcelService.PersonsToExcel(allPersons);
        }
    }
    
}

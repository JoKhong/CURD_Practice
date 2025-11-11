using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using OfficeOpenXml.ExternalReferences;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System.Formats.Asn1;
using System.Globalization;

namespace Services
{
    public class PersonServices : IPersonsServices
    {
        private readonly IPersonsRepository _personRepository;
        private readonly ICountriesService _countriesService;

        public PersonServices(IPersonsRepository personRepo, ICountriesService countriesService)
        {
            _personRepository = personRepo;
            _countriesService = countriesService;

            if (_countriesService == null)
                throw new ArgumentNullException(nameof(countriesService));
        }

        //private PersonResponse ConvertPersonToResponseWithCountry(Person person)
        //{
        //    PersonResponse response = person.ToPersonResponse();

        //    if(person.Country == null)
        //        response.Country = _countriesService.GetCountryById(person.CountryId)?.CountryName;
        //    else
        //        response.Country = person.Country.CountryName;
            
        //    return response;
        //}

        public async Task<PersonResponse> AddPerson(PersonAddRequest? addRequest)
        {
            if (addRequest == null)
                throw new ArgumentNullException();

            //Model Validation
            ValidationHelper.ModelValidation(addRequest);

            List<PersonResponse> results =  (await _personRepository.GetAllPersons())
                .Select(temp => temp.ToPersonResponse()).ToList();

            if (results.FirstOrDefault( temp => temp.PersonName == addRequest.PersonName ) is not null)
                throw new ArgumentException("Duplicate Names");

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

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            var persons = await _personRepository.GetAllPersons();

            return persons.Select(temp => temp.ToPersonResponse() ).ToList();
        }

        public async Task<PersonResponse?> GetPersonById(Guid? id)
        {
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
            List<Person>? persons = searchBy switch
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
           
            return persons.Select(temp => temp.ToPersonResponse()).ToList();

        }

        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            if(string.IsNullOrEmpty(sortBy))
                return allPersons;

            List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
            {
                (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) =>
                 allPersons = allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) =>
                 allPersons = allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.ASC) =>
                 allPersons = allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.DESC) =>
                 allPersons = allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.ASC) =>
                 allPersons = allPersons.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.DESC) =>
                 allPersons = allPersons.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.ASC) =>
                 allPersons = allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.DESC) =>
                 allPersons = allPersons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) =>
                 allPersons = allPersons.OrderBy(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) =>
                 allPersons = allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.ASC) =>
                 allPersons = allPersons.OrderBy(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.DESC) =>
                 allPersons = allPersons.OrderByDescending(temp => temp.Age).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) =>
                 allPersons = allPersons.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) =>
                 allPersons = allPersons.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),

                _ => allPersons
            };

            return sortedPersons;

        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? updateRequest)
        {
            if(updateRequest == null)
                throw new ArgumentNullException(nameof(updateRequest));

            ValidationHelper.ModelValidation(updateRequest);

            Person? matchingPerson = await _personRepository.GetPersonById(updateRequest.PersonId);

            if (matchingPerson == null)
                throw new ArgumentException("Given Id dose not exist");

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

        public async Task<bool> DeletePerson(Guid? personId)
        {
            if (personId == null || personId == Guid.Empty)
                return false;

            Person? matchingPerson = await _personRepository.GetPersonById(personId.Value);

            if (matchingPerson == null)
                return false;

            await _personRepository.DeletePersonsById(matchingPerson.PersonId);

            return true;
        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            //Get Data
            List<PersonResponse> response = await GetAllPersons();

            CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture , leaveOpen:true);

            
            //Prepare Header
            csvWriter.WriteHeader<PersonResponse>();
            csvWriter.NextRecord();

            //Fill in Data
            await csvWriter.WriteRecordsAsync(response);

            stream.Position = 0;
            return stream;

        }

        public async Task<MemoryStream> GetPersonsCSVCustom()
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            //Get Data
            List<PersonResponse> response = await GetAllPersons();

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

        public async Task<MemoryStream> GetPersonsExcel()
        {
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
                List<PersonResponse> response = await GetAllPersons();

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

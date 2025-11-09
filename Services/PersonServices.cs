using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
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
        private readonly ApplicationDbContext _db;
        private readonly ICountriesService _countriesService;

        public PersonServices(ApplicationDbContext personsDbContext, ICountriesService countriesService)
        {
            _db = personsDbContext;
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

            if(await _db.Persons.CountAsync( temp => temp.PersonName == addRequest.PersonName) > 0 )
                throw new ArgumentException("Duplicate Names");

            try 
            {
                Person person = addRequest.ToPerson();
                person.PersonId = Guid.NewGuid();

                _db.Persons.Add(person);
                await _db.SaveChangesAsync();

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
            var persons = await _db.Persons.Include("Country").ToListAsync();

            return persons.Select(temp => temp.ToPersonResponse() ).ToList();
        }

        public async Task<PersonResponse?> GetPersonById(Guid? id)
        {
            //throw new NotImplementedException();

            if (id == null)
                return null;

            Person? response = await _db.Persons.Include("Country").Where(temp =>  temp.PersonId == id).FirstOrDefaultAsync();

            if (response == null)
                return null;

            return response.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> allPersons = await GetAllPersons();
            List<PersonResponse> matchingPersons = allPersons;

            if(string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
                return matchingPersons;

            switch (searchBy) 
            {
                case nameof(PersonResponse.PersonName):
                    {
                        matchingPersons = allPersons.Where( 
                            aPerson => 
                            (!string.IsNullOrEmpty(aPerson.PersonName) ? 
                                aPerson.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true )).
                                ToList();
                        break;
                    }
                case nameof(PersonResponse.Email):
                    {
                        matchingPersons = allPersons.Where(
                            aPerson =>
                            (!string.IsNullOrEmpty(aPerson.Email) ?
                                aPerson.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)).
                                ToList();
                        break;
                    }

                case nameof(PersonResponse.Gender):
                    {
                        matchingPersons = allPersons.Where(
                            aPerson =>
                            (!string.IsNullOrEmpty(aPerson.Gender) ?
                                (aPerson.Gender.ToLower() == searchString.ToLower()) : true)).
                                ToList();
                        break;
                    }

                case nameof(PersonResponse.CountryId):
                    {
                        matchingPersons = allPersons.Where(
                            aPerson =>
                            (!string.IsNullOrEmpty(aPerson.Country) ?
                                aPerson.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)).
                                ToList();
                        break;
                    }

                case nameof(PersonResponse.Country):
                    {
                        matchingPersons = allPersons.Where(
                            aPerson =>
                            (!string.IsNullOrEmpty(aPerson.Country) ?
                                aPerson.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)).
                                ToList();
                        break;
                    }

                case nameof(PersonResponse.DateOfBirth):
                    {
                        matchingPersons = allPersons.Where(
                            aPerson =>
                            (aPerson.DateOfBirth != null) ?
                                aPerson.DateOfBirth.Value.ToString("dd MMMM yyyy").
                                Contains(searchString, StringComparison.OrdinalIgnoreCase) : true ).
                                ToList();
                        break;
                    }

                default:
                    {
                        matchingPersons = allPersons;
                        break;
                    }
            }

            return matchingPersons;

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

            Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonId == updateRequest.PersonId);

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

                await _db.SaveChangesAsync();//Update

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

            Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonId == personId);

            if (matchingPerson == null)
                return false;

            _db.Persons.Remove( _db.Persons.First( temp => temp.PersonId == personId) );
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            //Get Data
            List<PersonResponse> response = _db.Persons.Include("Country").Select(person => person.ToPersonResponse()).ToList();

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
            List<PersonResponse> response = _db.Persons.Include("Country").Select(person => person.ToPersonResponse()).ToList();

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
                List<PersonResponse> response = _db.Persons.Include("Country").Select(person => person.ToPersonResponse()).ToList();

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

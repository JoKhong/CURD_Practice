using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services
{
    public class PersonServices : IPersonsServices
    {
        private readonly List<Person> _persons;
        private readonly ICountriesService _countriesService;

        public PersonServices()
        {
            _persons = new List<Person>();
            _countriesService = new CountryServices(false);
        }

        private PersonResponse ConvertPersonToResponseWithCountry(Person person)
        {
            PersonResponse response = person.ToPersonResponse();
            response.Country = _countriesService.GetCountryById(person.CountryId)?.CountryName;

            return response;
        }


        public PersonResponse AddPerson(PersonAddRequest? addRequest)
        {
            if (addRequest == null)
                throw new ArgumentNullException();

            //Model Validation
            ValidationHelper.ModelValidation(addRequest);

            if(_persons.Where( temp => temp.PersonName == addRequest.PersonName).Count() > 0 )
                throw new ArgumentException("Duplicate Names");

            try 
            {
                Person person = addRequest.ToPerson();
                person.PersonID = Guid.NewGuid();
                _persons.Add(person);

                return ConvertPersonToResponseWithCountry(person);
            }
            catch(Exception ex)
            {
                if (addRequest == null)
                    throw new ArgumentNullException();

                return null;
            }
        }

        public List<PersonResponse> GetAllPersons()
        {
            return _persons.Select(temp => temp.ToPersonResponse()).ToList();
        }

        public PersonResponse? GetPersonById(Guid? id)
        {
            //throw new NotImplementedException();

            if (id == null)
                return null;

            Person? response = _persons.Where(temp =>  temp.PersonID == id).FirstOrDefault();

            if (response == null)
                return null;

            return ConvertPersonToResponseWithCountry(response);
        }

        public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> allPersons = GetAllPersons();
            List<PersonResponse> matchingPersons = allPersons;

            if(string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
                return matchingPersons;

            switch (searchBy) 
            {
                case nameof(Person.PersonName):
                    {
                        matchingPersons = allPersons.Where( 
                            aPerson => 
                            (!string.IsNullOrEmpty(aPerson.PersonName) ? 
                                aPerson.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true )).
                                ToList();
                        break;
                    }
                case nameof(Person.Email):
                    {
                        matchingPersons = allPersons.Where(
                            aPerson =>
                            (!string.IsNullOrEmpty(aPerson.Email) ?
                                aPerson.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)).
                                ToList();
                        break;
                    }

                case nameof(Person.Gender):
                    {
                        matchingPersons = allPersons.Where(
                            aPerson =>
                            (!string.IsNullOrEmpty(aPerson.Gender) ?
                                aPerson.Gender.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)).
                                ToList();
                        break;
                    }

                case nameof(Person.CountryId):
                    {
                        matchingPersons = allPersons.Where(
                            aPerson =>
                            (!string.IsNullOrEmpty(aPerson.Country) ?
                                aPerson.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)).
                                ToList();
                        break;
                    }

                case nameof(Person.DateOfBirth):
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

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            if(string.IsNullOrEmpty(sortBy))
                return allPersons;

            List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
            {
                (nameof(PersonResponse.PersonName), SortOrderOptions.AES) =>
                allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.AES) =>
               allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.AES) =>
                allPersons.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.AES) =>
               allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.AES) =>
                allPersons.OrderBy(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.AES) =>
               allPersons.OrderBy(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Age).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.AES) =>
              allPersons.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),

                _ => allPersons
            };

            return sortedPersons;

        }

        public PersonResponse UpdatePerson(PersonUpdateRequest? updateRequest)
        {
            if(updateRequest == null)
                throw new ArgumentNullException(nameof(updateRequest));

            ValidationHelper.ModelValidation(updateRequest);

            Person? matchingPerson = _persons.FirstOrDefault(temp => temp.PersonID == updateRequest.PersonId);

            if (matchingPerson == null)
                throw new ArgumentException("Given Id dose not exist");

            try 
            {
                matchingPerson.PersonName = updateRequest.PersonName;
                matchingPerson.Email = updateRequest.Email;
                //matchingPerson.DateOfBirth = updateRequest.DateOfBirth;
                //matchingPerson.Gender = updateRequest.Gender.ToString();
                //matchingPerson.CountryId = updateRequest.CountryId;
                //matchingPerson.Address = updateRequest.Address;
                //matchingPerson.ReceiveNewsLetters = updateRequest.ReceiveNewsLetters;

                return matchingPerson.ToPersonResponse();
            }
            catch (Exception ex) 
            {
                return null;
            }
        }

        public bool DeletePerson(Guid? PersonId)
        {
            if (PersonId == null || PersonId == Guid.Empty)
                return false;

            Person? matchingPerson = _persons.FirstOrDefault(temp => temp.PersonID == PersonId);

            if (matchingPerson == null)
                return false;

            _persons.Remove(matchingPerson);
            return true;
        }
    }
    
}

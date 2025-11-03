using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

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
            _countriesService = new CountryServices();
        }

        private PersonResponse ConvertPersonToResponseWithCountry(Person person)
        {
            PersonResponse response = person.ToPersonResponse();
            response.Country = _countriesService.GetCountryById(person.CountryID)?.CountryName;

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

                case nameof(Person.CountryID):
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
    }
}

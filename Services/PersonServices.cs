using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

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

            if (string.IsNullOrEmpty(addRequest.PersonName))
                throw new ArgumentException("Person name is null or empty");

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
    }
}

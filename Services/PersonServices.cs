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
            throw new NotImplementedException();
        }
    }
}

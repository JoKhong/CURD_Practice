using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountryServices : ICountriesService
    {
        private readonly PersonsDbContext _db;

        public CountryServices(PersonsDbContext personsDbContext)
        {
            _db = personsDbContext;
        }

        public CountryResponse AddCountry(CountryAddRequest? AddRequest)
        {
            if (AddRequest == null)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(AddRequest.CountryName))
            {
                throw new ArgumentException("AddRequest.CountryName is null or empty");
            }

            if (_db.Countries.Count(aCountry => aCountry.CountryName == AddRequest.CountryName) > 0)
            {
                throw new ArgumentException("Country already added");
            }

            try 
            {
                Country country = AddRequest.ToCountry();
                country.CountryId = Guid.NewGuid();

                _db.Countries.Add(country);
                _db.SaveChanges();

                return country.ToCountryResponse();
            }
            catch(Exception ex)
            {
                return null;
            }

        }

        public List<CountryResponse> GetAllCountries()
        {
            return _db.Countries.Select(country => country.ToCountryResponse()).ToList();
        }

        public CountryResponse? GetCountryById(Guid? id)
        {
            if(id == null)
                return null;

            Country? validCountry = _db.Countries.Where( country => country.CountryId == id).FirstOrDefault();

            if (validCountry == null)
                return null;

            return validCountry.ToCountryResponse();
        }
    }
}

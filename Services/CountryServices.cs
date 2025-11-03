using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountryServices : ICountriesService
    {
        private readonly List<Country> _countries;

        public CountryServices()
        {
            _countries = new List<Country>();
        }

        public CountryResponse AddCountry(CountryAddRequest? AddRequest)
        {
            if (AddRequest == null)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(AddRequest.CountryName))
            {
                throw new ArgumentException("AddRequest.CountryName is null or empty");
            }

            if (_countries.Where(aCountry => aCountry.CountryName == AddRequest.CountryName).Count() > 0)
            {
                throw new ArgumentException("Country already added");
            }

            try 
            {
                //Country country = new Country
                //{
                //    CountryId = Guid.NewGuid(),
                //    CountryName = AddRequest.CountryName
                //};

                Country country = AddRequest.ToCountry();
                country.CountryId = Guid.NewGuid();

                _countries.Add(country);

                return country.ToCountryResponse();
            }
            catch(Exception ex)
            {
                return null;
            }

        }

        public List<CountryResponse> GetAllCountries()
        {
            return _countries.Select(country => country.ToCountryResponse()).ToList();
        }

        public CountryResponse? GetCountryById(Guid? id)
        {
            if(id == null)
                return null;

            Country? validCountry = _countries.Where( country => country.CountryId == id).FirstOrDefault();

            if (validCountry == null)
                return null;

            return validCountry.ToCountryResponse();
        }
    }
}

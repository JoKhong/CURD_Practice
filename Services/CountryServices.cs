using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountryServices : ICountriesService
    {
        private readonly List<Country> _countries;

        public CountryServices(bool initialize)
        {
            _countries = new List<Country>();

            if(initialize)
            {
                _countries.AddRange(new List<Country>()
                {
                    new Country() {  CountryId = Guid.Parse("000C76EB-62E9-4465-96D1-2C41FDB64C3B"), CountryName = "USA" },
                    new Country() { CountryId = Guid.Parse("32DA506B-3EBA-48A4-BD86-5F93A2E19E3F"), CountryName = "Canada" },
                    new Country() { CountryId = Guid.Parse("DF7C89CE-3341-4246-84AE-E01AB7BA476E"), CountryName = "UK" },
                    new Country() { CountryId = Guid.Parse("15889048-AF93-412C-B8F3-22103E943A6D"), CountryName = "India" },
                    new Country() { CountryId = Guid.Parse("80DF255C-EFE7-49E5-A7F9-C35D7C701CAB"), CountryName = "Australia" }
                });
            }

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

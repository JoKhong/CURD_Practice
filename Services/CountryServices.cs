using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountryServices : ICountriesService
    {
        List<string> _countries;

        public CountryServices()
        {
            _countries = new List<string>();
        }

        public CountryResponse AddCountry(CountryAddRequest? AddRequest)
        {
            if (AddRequest != null)
            {
                if (string.IsNullOrEmpty(AddRequest.CountryName))
                {
                    throw new ArgumentException("AddRequest.CountryName is null or empty");
                }

                if (_countries.Contains(AddRequest.CountryName)) 
                {
                    throw new ArgumentException("Country already added");
                }
            }

            try 
            {
                _countries.Add(AddRequest.CountryName);

                return new CountryResponse()
                {
                    CountryId = Guid.NewGuid(),
                    CountryName = AddRequest.CountryName
                };
            }
            catch(Exception ex)
            {
                if (AddRequest == null)
                    throw new ArgumentNullException();

                return null;
            }

        }
    }
}

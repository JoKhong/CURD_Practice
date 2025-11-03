using Entities;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO class for adding new country
    /// </summary>
    public class CountryAddRequest
    {
        public string? CountryName { get; set; }

        public Country ToCountry()
        {
            Country country = new Country() {
                CountryName = CountryName,
            };

            return country;
        }
    }
}

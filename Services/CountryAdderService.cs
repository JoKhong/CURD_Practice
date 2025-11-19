using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using System.Diagnostics.Metrics;

namespace Services
{
    public class CountryAdderService : ICountryAdderService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountryAdderService(ICountriesRepository countriesRepo)
        {
            _countriesRepository = countriesRepo;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? AddRequest)
        {
            if (AddRequest == null)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(AddRequest.CountryName))
            {
                throw new ArgumentException("AddRequest.CountryName is null or empty");
            }

            if (await _countriesRepository.GetCountryByName(AddRequest.CountryName) is not null)
            {
                throw new ArgumentException("Country already added");
            }

            try 
            {
                Country country = AddRequest.ToCountry();
                country.CountryId = Guid.NewGuid();

                await _countriesRepository.AddCountry(country);

                return country.ToCountryResponse();
            }
            catch(Exception ex)
            {
                return null;
            }

        }

    }
}

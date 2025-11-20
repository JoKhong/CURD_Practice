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
    public class CountryGetCountriesService : ICountryGetCountriesService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountryGetCountriesService(ICountriesRepository countriesRepo)
        {
            _countriesRepository = countriesRepo;
        }

        public async Task<List<CountryResponse>> GetCountriesAll()
        {
            List<Country> countries = await _countriesRepository.GetAllCountries();

            return countries.Select(temp => temp.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryById(Guid? id)
        {
            if (id == null)
                return null;

            Country? validCountry = await _countriesRepository.GetCountryById(id.Value);

            if (validCountry == null)
                return null;

            return validCountry.ToCountryResponse();
        }
    }
}

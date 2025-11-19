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
    public class CountryGetAllCountriesService : ICountryGetAllCountriesService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountryGetAllCountriesService(ICountriesRepository countriesRepo)
        {
            _countriesRepository = countriesRepo;
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            List<Country> countries = await _countriesRepository.GetAllCountries();

            return countries.Select(temp => temp.ToCountryResponse()).ToList();
        }

    }
}

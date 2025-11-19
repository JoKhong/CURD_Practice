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
    public class CountryGetByIdService : ICountryGetByIdService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountryGetByIdService(ICountriesRepository countriesRepo)
        {
            _countriesRepository = countriesRepo;
        }

        public async Task<CountryResponse?> GetCountryById(Guid? id)
        {
            if(id == null)
                return null;

            Country? validCountry = await _countriesRepository.GetCountryById(id.Value);

            if (validCountry == null)
                return null;

            return  validCountry.ToCountryResponse();
        }
    }
}

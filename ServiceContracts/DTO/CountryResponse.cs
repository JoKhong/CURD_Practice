using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entities;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO used as return type for Countries Serivce
    /// </summary>
    public class CountryResponse
    {
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }
    }

    public static class CountryExtensions
    {
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse
            {
                CountryId = country.CountryId,
                CountryName = country.CountryName,
            };
        }
    }

}

using Entities;
using ServiceContracts.Enums;
using System;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO for PersonResponse
    /// </summary>
    public class PersonResponse
    {
        public Guid PersonId { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? CountryId { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public double? Age { get; set; }

        public override bool Equals(object? obj)
        {
            if(obj == null)
                return false;

            if(obj.GetType() !=  typeof(PersonResponse))
                return false;

            PersonResponse other = (PersonResponse)obj;

            bool result = (
                this.PersonId == other.PersonId &&
                this.PersonName == other.PersonName &&
                this.Email == other.Email &&
                this.CountryId == other.CountryId &&
                this.Country == other.Country &&
                this.DateOfBirth == other.DateOfBirth &&
                this.Gender == other.Gender &&
                this.Address == other.Address &&
                this.ReceiveNewsLetters == other.ReceiveNewsLetters
                );

            return result;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Person ID: {PersonId}, " +
                $"Person Name: {PersonName}, " +
                $"Email: {Email}, " +
                $"Date of Birth: {DateOfBirth?.ToString("dd MMM yyyy")}, " +
                $"Gender: {Gender}, " +
                $"Country ID: {CountryId?.ToString()}, " +
                $"Country: {Country}, " +
                $"Address: {Address}, " +
                $"Receive News Letters: {ReceiveNewsLetters}";
        }

        public PersonUpdateRequest ToPersonUpdateRequest()
        {
            PersonUpdateRequest ret = new PersonUpdateRequest()
            {
                PersonId = PersonId,
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth,
                Gender = (SexOptions)Enum.Parse(typeof(SexOptions), Gender, true),
                CountryId = CountryId,
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters,
            };

            return ret;
        }

    }

    public static class PersonExtension
    {
        /// <summary>
        /// Extension method for converting Person object to PersonResponse
        /// </summary>
        /// <param name="person"></param>
        /// <returns>Return PersonResponse from a Person</returns>
        public static PersonResponse ToPersonResponse(this Person person)
        {
            PersonResponse response = new PersonResponse()
            {
                PersonId = person.PersonID,
                PersonName = person.PersonName,
                Email = person.Email,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                CountryId = person.CountryId,
                Address = person.Address,
                ReceiveNewsLetters = person.ReceiveNewsLetters,
                Age = (person.DateOfBirth != null) ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25) : null
            };

            return response;
        }
    }

}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    public class Person
    {
        [Key]
        public Guid PersonId { get; set; }

        [StringLength(40)]
        public string? PersonName { get; set; }

        [StringLength(40)]
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        //Unique ID
        public Guid? CountryId { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        //bit
        public bool ReceiveNewsLetters { get; set; }

        public string? TIN { get; set; }

        [ForeignKey(nameof(Country.CountryId))]
        public virtual Country? Country { get; set; }


        public override string ToString()
        {
            string toString = $"{nameof(PersonId)}: {PersonId}, " +
                $"{nameof(PersonName)}: {PersonName}, " +
                $"{nameof(Email)}: {Email}, " +
                $"{nameof(DateOfBirth)}: {DateOfBirth?.ToString("MM/dd/yyyy")}, " +
                $"{nameof(Gender)}: {Gender}, " +
                $"{nameof(CountryId)}: {CountryId}, " +
                $"{nameof(Country)}: {Country}, " +
                $"{nameof(Address)}: {Address}, " +
                $"{nameof(ReceiveNewsLetters)}: {ReceiveNewsLetters} ";

            return toString;
        }

    }

    

}

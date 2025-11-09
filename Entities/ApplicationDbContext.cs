using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Entities
{
    public class ApplicationDbContext : DbContext 
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<Person> Persons { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");

            //Seed countires
            try
            {
                
                //string countiresJson = File.ReadAllText("countries.json");
                //List<Country> countries = JsonSerializer.Deserialize<List<Country>>(countiresJson);

                //foreach (Country country in countries)
                //    modelBuilder.Entity<Country>().HasData(country);

                //string personsJson = File.ReadAllText("persons.json");
                //List<Person> persons = JsonSerializer.Deserialize<List<Person>>(countiresJson);

                //foreach (Person peson in persons)
                //    modelBuilder.Entity<Country>().HasData(peson);
            }
            catch (Exception ex) { }

            //Fluent API
            try
            {
                modelBuilder.Entity<Person>().Property(temp => temp.TIN)
                  .HasColumnName("TaxIdentificationNumber")
                  .HasColumnType("varchar(8)");

                //modelBuilder.Entity<Person>()
                //  .HasIndex(temp => temp.TIN).IsUnique();

                modelBuilder.Entity<Person>()
                    .ToTable( Temp => Temp.HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8"));
            }
            catch (Exception ex) { }

        }

        public List<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
        }

        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[] {
        new SqlParameter("@PersonId", person.PersonId),
        new SqlParameter("@PersonName", person.PersonName),
        new SqlParameter("@Email", person.Email),
        new SqlParameter("@DateOfBirth", person.DateOfBirth),
        new SqlParameter("@Gender", person.Gender),
        new SqlParameter("@CountryId", person.CountryId),
        new SqlParameter("@Address", person.Address),
        new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters)
      };

            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonId, @PersonName, @Email, @DateOfBirth, @Gender, @CountryId, @Address, @ReceiveNewsLetters", parameters);
        }

    }
}

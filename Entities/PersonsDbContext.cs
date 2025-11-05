using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class PersonsDbContext : DbContext 
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<Person> Persons { get; set; }

        public PersonsDbContext(DbContextOptions options) : base(options)
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

        }

    }
}

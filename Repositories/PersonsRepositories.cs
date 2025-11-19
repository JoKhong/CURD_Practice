using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class PersonsRepositories : IPersonsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PersonsRepositories> _logger;

        public PersonsRepositories(ApplicationDbContext db, ILogger<PersonsRepositories> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Person> AddPerson(Person person)
        {
            _logger.LogInformation("AddPerson of PersonsRepositories");

            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return person;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonsRepositories");
            return await _db.Persons.Include("Country").ToListAsync();
        }

        public async Task<Person?> GetPersonById(Guid personId)
        {
            _logger.LogInformation("GetPersonById of PersonsRepositories");

            return await _db.Persons.Include("Country")
                .FirstOrDefaultAsync(temp => temp.PersonId == personId);

        }

        public async Task<List<Person>?> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            _logger.LogInformation("GetFilteredPersons of PersonsRepositories");

            return await _db.Persons.Include("Country")
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Person> UpdatePerson(Person person)
        {
            _logger.LogInformation("UpdatePerson of PersonsRepositories");

            Person? mathcingPerson = await _db.Persons.Include("Country").FirstOrDefaultAsync(temp => temp.PersonId == person.PersonId);
            
            if(mathcingPerson == null)
                return person;

            mathcingPerson.PersonName = person.PersonName;
            mathcingPerson.Email = person.Email;
            mathcingPerson.DateOfBirth = person.DateOfBirth;
            mathcingPerson.Gender = person.Gender;
            mathcingPerson.Country = person.Country;
            mathcingPerson.CountryId = person.CountryId;
            mathcingPerson.Address = person.Address;
            mathcingPerson.ReceiveNewsLetters = person.ReceiveNewsLetters;
            mathcingPerson.TIN = person.TIN;

            await _db.SaveChangesAsync();

            return mathcingPerson;

        }

        public async Task<bool> DeletePersonsById(Guid personId)
        {
            _logger.LogInformation("DeletePersonsById of PersonsRepositories");

            _db.Persons.RemoveRange( _db.Persons.Where( temp => temp.PersonId == personId) );
            int rowsDeleted = await _db.SaveChangesAsync();

            return rowsDeleted > 0;
        }
    }
}

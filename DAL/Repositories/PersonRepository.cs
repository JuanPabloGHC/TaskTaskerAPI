using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class PersonRepository : IPersonRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        private enum Columns { PHONE_COLUMN, NAME_COLUMN };

        #endregion

        #region CONSTRUCTOR

        public PersonRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<Person?> GetPersonByID(int id)
        {
            return await this._context.Persons.FindAsync(id);
        }

        public async Task<Person> CreatePerson(PersonDTO personDTO)
        {
            if (this.Exists(Columns.PHONE_COLUMN, personDTO.phone))
                throw new Exception("409;Phone already in use");

            if (this.Exists(Columns.NAME_COLUMN, personDTO.name))
                throw new Exception("409;Name already in use");

            Person person = new Person(personDTO);

            await this._context.AddAsync(person);

            return person;
        }

        public async Task<Person> UpdatePerson(PersonDTO personDTO)
        {
            Person? person = await this.GetPersonByID(personDTO.id);

            if (person == null)
                throw new Exception("404;User not found");

            if (this.Exists(Columns.PHONE_COLUMN, personDTO.phone, personDTO.id))
                throw new Exception("409;Phone already in use");

            if (this.Exists(Columns.NAME_COLUMN, personDTO.name, personDTO.id))
                throw new Exception("409;Name already in use");

            person.phone = personDTO.phone;

            person.name = personDTO.name;

            person.password = personDTO.password;

            person.image = personDTO.image;

            this._context.Entry(person).State = EntityState.Modified;

            return person;
        }

        public async Task DeletePerson(int id)
        {
            Person? person = await this.GetPersonByID(id);

            if (person == null)
                throw new Exception("404;User not found");

            this._context.Remove(person);
        }

        public async Task SaveChanges()
        {
            await this._context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this._context.Dispose();
                }
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        #region PRIVATE METHODS

        private bool Exists(Columns column, string value, int idException = -1)
        {
            switch (column)
            {
                case Columns.PHONE_COLUMN:
                    return this._context.Persons
                        .Where(p => p.phone == value && p.id != idException)
                        .Any();
                case Columns.NAME_COLUMN:
                    return this._context.Persons
                        .Where(p => p.name == value && p.id != idException)
                        .Any();
                default:
                    return false;
            }
        }

        #endregion

    }
}

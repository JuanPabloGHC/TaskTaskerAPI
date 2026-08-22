using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IPersonRepository : IDisposable
    {
        Task<Person?> GetPersonByID(int id);
        Task<Person?> GetPersonByName(string name);
        Task<Person> CreatePerson(PersonDTO personDTO);
        Task<Person> UpdatePerson(PersonDTO personDTO);
        Task DeletePerson(int id);
        Task SaveChanges();
    }
}

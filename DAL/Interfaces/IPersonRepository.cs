using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IPersonRepository : IDisposable
    {
        Person GetPersonByID(int id);
        Task CreatePerson(PersonDTO personDTO);
        Task UpdatePerson(PersonDTO personDTO);
        Task DeletePerson(int id);
        Task SaveChanges();
    }
}

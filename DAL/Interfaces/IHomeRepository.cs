using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IHomeRepository : IDisposable
    {
        Task<Home?> GetHomeByID(int id);
        Task<Home> CreateHome(HomeDTO homeDTO, PersonDTO personDTO);
        Task<Home> UpdateHome(HomeDTO homeDTO);
        Task DeleteHome(int id);
        Task SaveChanges();
    }
}

using TaskTaskerAPI.DAL.DTOs;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IHomeRepository : IDisposable
    {
        Task CreateHome(HomeDTO homeDTO);
        Task UpdateHome(HomeDTO homeDTO);
        Task DeleteHome(int id);
        Task SaveChanges();
    }
}

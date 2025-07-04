using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IStatusRepository : IDisposable
    {
        Task<IEnumerable<Status>> GetAllStatuses();
        Task<Status?> GetStatusByID(int id);
        Task CreateStatus(StatusDTO statusDTO);
        Task UpdateStatus(StatusDTO statusDTO);
        Task DeleteStatus(int id);
        Task SaveChanges();
    }
}

using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface ITaskRepository : IDisposable
    {
        Task<IEnumerable<Entities.Task>> GetAllTasks();
        Task CreateTask(TaskDTO taskDTO);
        Task UpdateTask(TaskDTO taskDTO);
        Task DeleteTask(int id);
        Task SaveChanges();
    }
}

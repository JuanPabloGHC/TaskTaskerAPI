using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IAdminUserRepository : IDisposable
    {
        Task<AdminUser?> GetByUsername(string username);
        Task<bool> AnyAdmins();
        Task<AdminUser> CreateAdmin(string username, string password);
        Task DeleteAdmin(int id);
        Task SaveChanges();
    }
}

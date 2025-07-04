using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IRoleRepository : IDisposable
    {
        Task<IEnumerable<Role>> GetAllRoles();
        Task<Role?> GetRoleByID(int id);
        Task CreateRole(RoleDTO roleDTO);
        Task UpdateRole(RoleDTO roleDTO);
        Task DeleteRole(int id);
        Task SaveChanges();
    }
}

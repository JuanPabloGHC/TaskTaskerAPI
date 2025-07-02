using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllRoles();
        Task CreateRole(RoleDTO roleDTO);
        Task UpdateRole(RoleDTO roleDTO);
        Task DeleteRole(int id);
        Task SaveChanges();
    }
}

using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class RoleRepository : IRoleRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        #endregion

        #region CONSTRUCTOR

        public RoleRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            return await this._context.Roles
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByID(int id)
        {
            return await this._context.Roles
                .Where(r => r.id == id)
                .FirstOrDefaultAsync();
        }

        public async Task CreateRole(RoleDTO roleDTO)
        {
            if (this.Exists(roleDTO.name))
                throw new Exception("409;Name already in use");

            Role role = new Role(roleDTO);

            await this._context.AddAsync(role);
        }

        public async Task UpdateRole(RoleDTO roleDTO)
        {
            Role? role = await this.GetRoleByID(roleDTO.id);

            if (role == null)
                throw new Exception("404;Role not found");

            if (this.Exists(roleDTO.name, roleDTO.id))
                throw new Exception("409;Name already in use");

            role.name = roleDTO.name;

            role.image = roleDTO.image;

            this._context.Entry(role).State = EntityState.Modified;
        }

        public async Task DeleteRole(int id)
        {
            Role? role = await this.GetRoleByID(id);

            if (role == null)
                throw new Exception("404;Role not found");

            this._context.Remove(role);
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

        private bool Exists(string name, int idException = -1)
        {
            return this._context.Roles
                .Where(r => r.name == name && r.id != idException)
                .Any();
        }

        #endregion

    }
}

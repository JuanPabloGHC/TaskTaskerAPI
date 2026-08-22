using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class AdminUserRepository : IAdminUserRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        private readonly IPasswordHasher<AdminUser> _passwordHasher = new PasswordHasher<AdminUser>();

        #endregion

        #region CONSTRUCTOR

        public AdminUserRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<AdminUser?> GetByUsername(string username)
        {
            return await this._context.AdminUsers
                .Where(a => a.username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AnyAdmins()
        {
            return await this._context.AdminUsers.AnyAsync();
        }

        public async Task<AdminUser> CreateAdmin(string username, string password)
        {
            if (await this.GetByUsername(username) != null)
                throw new Exception("409;Username already in use");

            AdminUser admin = new AdminUser
            {
                username = username
            };

            admin.password = this._passwordHasher.HashPassword(admin, password);

            await this._context.AdminUsers.AddAsync(admin);

            return admin;
        }

        public async Task DeleteAdmin(int id)
        {
            AdminUser? admin = await this._context.AdminUsers.FindAsync(id);

            if (admin == null)
                throw new Exception("404;Admin not found");

            this._context.AdminUsers.Remove(admin);
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

    }
}

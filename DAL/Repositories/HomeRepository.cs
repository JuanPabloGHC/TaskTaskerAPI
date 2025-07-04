using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class HomeRepository : IHomeRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        #endregion

        #region CONSTRUCTOR

        public HomeRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<Home?> GetHomeByID(int id)
        {
            return await this._context.Homes
                .Where(h => h.id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Home> CreateHome(HomeDTO homeDTO)
        {
            if (this.Exists(homeDTO.name))
                throw new Exception("409;Name already in use");

            Home home = new Home(homeDTO);

            await this._context.AddAsync(home);

            return home;
        }

        public async Task<Home> UpdateHome(HomeDTO homeDTO)
        {
            Home? home = await this.GetHomeByID(homeDTO.id);

            if (home == null)
                throw new Exception("404;Home not found");

            if (this.Exists(homeDTO.name, homeDTO.id))
                throw new Exception("409;Name already in use");

            home.name = homeDTO.name;

            home.image = homeDTO.image;

            this._context.Entry(home).State = EntityState.Modified;

            return home;
        }

        public async Task DeleteHome(int id)
        {
            Home? home = await this.GetHomeByID(id);

            if (home == null)
                throw new Exception("404;Home not found");

            this._context.Remove(home);
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
            return this._context.Homes
                .Where(h => h.name == name && h.id != idException)
                .Any();
        }

        #endregion

    }
}

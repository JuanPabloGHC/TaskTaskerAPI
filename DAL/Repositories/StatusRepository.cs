using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class StatusRepository : IStatusRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        #endregion

        #region CONSTRUCTOR

        public StatusRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Status>> GetAllStatuses()
        {
            return await this._context.Statuses
                .ToListAsync();
        }

        public async Task<Status?> GetStatusByID(int id)
        {
            return await this._context.Statuses
                .Where(s => s.id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Status?> GetStatusByName(string name)
        {
            return await this._context.Statuses
                .Where(s => s.name == name)
                .FirstOrDefaultAsync();
        }

        public async Task CreateStatus(StatusDTO statusDTO)
        {
            Validate.Text("Name", statusDTO.name, 25);
            Validate.Text("Color", statusDTO.color, 7);

            if (this.Exists(statusDTO.name))
                throw new Exception("409;Name already in use");

            Status status = new Status(statusDTO);

            await this._context.AddAsync(status);
        }

        public async Task UpdateStatus(StatusDTO statusDTO)
        {
            Status? status = await this.GetStatusByID(statusDTO.id);

            if (status == null)
                throw new Exception("404;Status not found");

            Validate.Text("Name", statusDTO.name, 25);
            Validate.Text("Color", statusDTO.color, 7);

            if (this.Exists(statusDTO.name, statusDTO.id))
                throw new Exception("409;Name already in use");

            status.name = statusDTO.name;

            status.color = statusDTO.color;

            this._context.Entry(status).State = EntityState.Modified;
        }

        public async Task DeleteStatus(int id)
        {
            Status? status = await this.GetStatusByID(id);

            if (status == null)
                throw new Exception("404;Status not found");

            if (await this._context.Assignments.AnyAsync(a => a.status_id == id))
                throw new Exception("409;Status is in use and cannot be deleted");

            this._context.Remove(status);
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
            return this._context.Statuses
                .Where(s => s.name == name && s.id != idException)
                .Any();
        }

        #endregion

    }
}

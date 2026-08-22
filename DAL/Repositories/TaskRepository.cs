using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class TaskRepository : ITaskRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        #endregion

        #region CONSTRUCTOR

        public TaskRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Entities.Task>> GetAllTasks()
        {
            return await this._context.Tasks
                .ToListAsync();
        }

        public async Task<Entities.Task?> GetTaskByID(int id)
        {
            return await this._context.Tasks
                .Where(t => t.id == id)
                .FirstOrDefaultAsync();
        }

        public async Task CreateTask(TaskDTO taskDTO)
        {
            Validate.Text("Name", taskDTO.name, 25);
            Validate.Required("Image", taskDTO.image);

            if (this.Exists(taskDTO.name))
                throw new Exception("409;Name already in use");

            Entities.Task task = new Entities.Task(taskDTO);

            await this._context.AddAsync(task);
        }

        public async Task UpdateTask(TaskDTO taskDTO)
        {
            Entities.Task? task = await this.GetTaskByID(taskDTO.id);

            if (task == null)
                throw new Exception("404;Task not found");

            Validate.Text("Name", taskDTO.name, 25);
            Validate.Required("Image", taskDTO.image);

            if (this.Exists(taskDTO.name, taskDTO.id))
                throw new Exception("409;Name already in use");

            task.name = taskDTO.name;

            task.image = taskDTO.image;

            this._context.Entry(task).State = EntityState.Modified;
        }

        public async Task DeleteTask(int id)
        {
            Entities.Task? task = await this.GetTaskByID(id);

            if (task == null)
                throw new Exception("404;Task not found");

            if (await this._context.Assignments.AnyAsync(a => a.task_id == id)
                || await this._context.Achievements.AnyAsync(a => a.task_id == id))
                throw new Exception("409;Task is in use and cannot be deleted");

            this._context.Remove(task);
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
            return this._context.Tasks
                .Where(t => t.name == name && t.id != idException)
                .Any();
        }

        #endregion

    }
}

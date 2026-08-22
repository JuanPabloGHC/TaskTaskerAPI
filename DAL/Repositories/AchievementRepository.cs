using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class AchievementRepository : IAchievementRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        private readonly ITaskRepository _taskRepository;

        #endregion

        #region CONSTRUCTOR

        public AchievementRepository(TaskTaskerContext context, ITaskRepository taskRepository)
        {
            this._context = context;
            this._taskRepository = taskRepository;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Achievement>> GetAllAchievements()
        {
            return await this._context.Achievements
                .Include(a => a.task)
                .ToListAsync();
        }

        public async Task<Achievement?> GetAchievementByID(int id)
        {
            return await this._context.Achievements
                .Include(a => a.task)
                .Where(a => a.id == id)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAchievement(AchievementDTO achievementDTO)
        {
            Validate.Text("Name", achievementDTO.name, 50);
            Validate.Positive("Days", achievementDTO.days);
            Validate.Required("Image", achievementDTO.image);

            if (this.Exists(achievementDTO.name))
                throw new Exception("409;Name already in use");

            if (await this._taskRepository.GetTaskByID(achievementDTO.task.id) == null)
                throw new Exception("404;Task not found");

            Achievement achievement = new Achievement(achievementDTO);

            await this._context.AddAsync(achievement);
        }

        public async Task UpdateAchievement(AchievementDTO achievementDTO)
        {
            Achievement? achievement = await this.GetAchievementByID(achievementDTO.id);

            if (achievement == null)
                throw new Exception("404;Achievement not found");

            Validate.Text("Name", achievementDTO.name, 50);
            Validate.Positive("Days", achievementDTO.days);
            Validate.Required("Image", achievementDTO.image);

            if (this.Exists(achievementDTO.name, achievementDTO.id))
                throw new Exception("409;Name already in use");

            if (await this._taskRepository.GetTaskByID(achievementDTO.task.id) == null)
                throw new Exception("404;Task not found");

            achievement.name = achievementDTO.name;

            achievement.days = achievementDTO.days;

            achievement.image = achievementDTO.image;

            achievement.task_id = achievementDTO.task.id;

            this._context.Entry(achievement).State = EntityState.Modified;
        }

        public async Task DeleteAchievement(int id)
        {
            Achievement? achievement = await this.GetAchievementByID(id);

            if (achievement == null)
                throw new Exception("404;Achievement not found");

            if (await this._context.Attainments.AnyAsync(at => at.achievement_id == id))
                throw new Exception("409;Achievement is in use and cannot be deleted");

            this._context.Remove(achievement);
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
            return this._context.Achievements
                .Where(a => a.name == name && a.id != idException)
                .Any();
        }

        #endregion

    }
}

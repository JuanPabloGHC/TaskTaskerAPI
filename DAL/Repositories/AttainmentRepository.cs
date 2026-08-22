using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class AttainmentRepository : IAttainmentRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        #endregion

        #region CONSTRUCTOR

        public AttainmentRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Attainment>> GetMemberAttainmentes(int memberID)
        {
            return await this._context.Attainments
                .Where(a => a.member_id == memberID)
                .Include(a => a.achievement).ThenInclude(ach => ach.task)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attainment>> GetHomeAttainmentes(int homeID)
        {
            return await this._context.Attainments
                .Where(a => a.member.home_id == homeID)
                .Include(a => a.member).ThenInclude(m => m.person)
                .Include(a => a.achievement).ThenInclude(ach => ach.task)
                .ToListAsync();
        }

        public async Task CreateAttainment(int memberID, int achievementID)
        {
            if (this.Exists(memberID, achievementID))
                throw new Exception("409;Attainment already exists");

            MemberRepository memberRepository = new MemberRepository(this._context);

            AchievementRepository achievementRepository  = new AchievementRepository(this._context);

            if (await memberRepository.GetMemberByID(memberID) == null)
                throw new Exception("404;Member not found");

            if (await achievementRepository.GetAchievementByID(achievementID) == null)
                throw new Exception("404;Task not found");

            Attainment attainment = new Attainment(0, memberID, achievementID);

            await this._context.AddAsync(attainment);
        }

        public async Task<List<Achievement>> ValidateAchievement(int memberID, int taskID)
        {
            AchievementRepository achievementRepository = new AchievementRepository(this._context);

            return await this.GetTaskAchievements(memberID, taskID);
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

        private bool Exists(int memberID, int achievementID)
        {
            return this._context.Attainments
                .Where(a => a.member_id == memberID && a.achievement_id == achievementID)
                .Any();
        }

        private async Task<List<Achievement>> GetTaskAchievements(int memberId, int taskId)
        {
            int completedDays = await this._context.Assignments
                .Include(a => a.status)
                .Where(a => a.member_id == memberId && a.task_id == taskId && a.status.name == "Done")
                .CountAsync();

            return await this._context.Achievements
                .Include(a => a.task)
                .Where(a => a.task_id == taskId
                    && a.days <= completedDays
                    && !this._context.Attainments
                        .Any(at => at.member_id == memberId && at.achievement_id == a.id))
                .ToListAsync();
        }

        #endregion

    }
}

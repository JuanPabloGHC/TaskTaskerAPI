using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
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
                .ToListAsync();
        }

        public async Task CreateAttainment(int memberID, int achievementID)
        {
            if (this.Exists(memberID, achievementID))
                throw new Exception("409;Attainment already exists");

            Attainment attainment = new Attainment(0, memberID, achievementID);

            await this._context.AddAsync(attainment);
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

        #endregion

    }
}

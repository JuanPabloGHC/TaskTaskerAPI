using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        #endregion

        #region CONSTRUCTOR

        public RefreshTokenRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            await this._context.RefreshTokens.AddAsync(refreshToken);
        }

        public async Task<RefreshToken?> GetByToken(string token)
        {
            return await this._context.RefreshTokens
                .Where(rt => rt.token == token)
                .FirstOrDefaultAsync();
        }

        public void RevokeToken(RefreshToken refreshToken)
        {
            refreshToken.is_revoked = true;

            this._context.Entry(refreshToken).State = EntityState.Modified;
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

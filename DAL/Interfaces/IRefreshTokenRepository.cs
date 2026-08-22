using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IRefreshTokenRepository : IDisposable
    {
        Task AddRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken?> GetByToken(string token);
        void RevokeToken(RefreshToken refreshToken);
        Task SaveChanges();
    }
}

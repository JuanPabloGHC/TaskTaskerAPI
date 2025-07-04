using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IAchievementRepository : IDisposable
    {
        Task<IEnumerable<Achievement>> GetAllAchievements();
        Task CreateAchievement(AchievementDTO achievementDTO);
        Task UpdateAchievement(Achievement achievementDTO);
        Task DeleteAchievement(Achievement achievement);
        Task SaveChanges();
    }
}

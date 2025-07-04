using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IAchievementRepository : IDisposable
    {
        Task<IEnumerable<Achievement>> GetAllAchievements();
        Task<Achievement?> GetAchievementByID(int id);
        Task CreateAchievement(AchievementDTO achievementDTO);
        Task UpdateAchievement(AchievementDTO achievementDTO);
        Task DeleteAchievement(int id);
        Task SaveChanges();
    }
}

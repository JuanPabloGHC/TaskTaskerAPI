using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IAttainmentRepository : IDisposable
    {
        Task<IEnumerable<Attainment>> GetMemberAttainmentes(int memberID);
        Task<(IEnumerable<Attainment> items, int total)> GetHomeAttainmentesPaged(int homeID, int page, int pageSize);
        Task CreateAttainment(int memberID, int achievementID);
        Task<List<Achievement>> ValidateAchievement(int memberID, int taskID);
        Task SaveChanges();
    }
}

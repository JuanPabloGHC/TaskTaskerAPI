using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IAttainmentRepository : IDisposable
    {
        Task<IEnumerable<Attainment>> GetMemberAttainmentes(int memberID);
        Task CreateAttainment(int memberID, int achievementID);
    }
}

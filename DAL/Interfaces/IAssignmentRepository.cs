using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IAssignmentRepository : IDisposable
    {
        Task<IEnumerable<Assignment>> GetHomeAssignments(int homeID);
        Task<IEnumerable<Assignment>> GetMemberAssignments(int memberID);
        Task<IEnumerable<Assignment>> GetUndoneMemberAssignments(int memberID);
        Task<Assignment?> GetAssignmentByID(int id);
        Task CreateAssignment(AssignmentDTO assignmentDTO, int adminId);
        Task UpdateAssignment(AssignmentDTO assignmentDTO, int memberId);
        Task DeleteAssignment(int id, int adminId);
        Task SaveChanges();
    }
}

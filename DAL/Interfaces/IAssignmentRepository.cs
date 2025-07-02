using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<IEnumerable<Assignment>> GetHomeAssignments(int homeID);
        Task<IEnumerable<Assignment>> GetMemberAssignments(int memberID);
        Task CreateAssignment(AssignmentDTO assignmentDTO);
        Task UpdateAssignment(AssignmentDTO assignmentDTO);
        Task DeleteAssignment(int id);
    }
}

using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class AssignmentRepository : IAssignmentRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        #endregion

        #region CONSTRUCTOR

        public AssignmentRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Assignment>> GetHomeAssignments(int homeID)
        {
            return await this._context.Assignments
                .Where(a => a.member.home_id == homeID)
                .Include(a => a.task)
                .Include(a => a.member)
                .Include(a => a.member.person)
                .Include(a => a.member.role)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetMemberAssignments(int memberID)
        {
            return await this._context.Assignments
                .Where(a => a.member_id == memberID)
                .Include(a => a.task)
                .Include(a => a.status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetUndoneMemberAssignments(int memberID)
        {
            return await this._context.Assignments
                .Include(a => a.task)
                .Include(a => a.status)
                .Where(a => a.member_id == memberID && a.status.name != "Done")
                .ToListAsync();
        }

        public async Task<Assignment?> GetAssignmentByID(int id)
        {
            return await this._context.Assignments
                .Where(a => a.id == id)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAssignment(AssignmentDTO assignmentDTO, int adminId)
        {
            MemberRepository memberRepository = new MemberRepository(this._context);

            Member? member = await memberRepository.GetMemberByID(adminId);

            if (member == null || (member.role.name != "Admin" && member.role.name != "Owner"))
                throw new Exception("403;You do not have permission");

            if (this.Exists(assignmentDTO.member.id, assignmentDTO.task.id, assignmentDTO.date))
                throw new Exception("409;Assignment already exists");

            TaskRepository taskRepository = new TaskRepository(this._context);

            StatusRepository statusRepository = new StatusRepository(this._context);

            if (await memberRepository.GetMemberByID(assignmentDTO.member.id) == null)
                throw new Exception("404;Member not found");

            if (await taskRepository.GetTaskByID(assignmentDTO.task.id) == null)
                throw new Exception("404;Task not found");

            if (await statusRepository.GetStatusByID(assignmentDTO.status.id) == null)
                throw new Exception("404;Status not found");

            Assignment assignment = new Assignment(assignmentDTO);

            await this._context.Assignments.AddAsync(assignment);
        }

        public async Task UpdateAssignment(AssignmentDTO assignmentDTO, int adminId)
        {
            MemberRepository memberRepository = new MemberRepository(this._context);

            Member? member = await memberRepository.GetMemberByID(adminId);

            if (member == null || (member.role.name != "Admin" && member.role.name != "Owner"))
                throw new Exception("403;You do not have permission");

            Assignment? assignment = await this.GetAssignmentByID(assignmentDTO.id);

            if (assignment == null)
                throw new Exception("404;Assignment not found");

            if (this.Exists(assignmentDTO.member.id, assignmentDTO.task.id, assignmentDTO.date))
                throw new Exception("409;Assignment already exists");

            TaskRepository taskRepository = new TaskRepository(this._context);

            StatusRepository statusRepository = new StatusRepository(this._context);

            if (await taskRepository.GetTaskByID(assignmentDTO.task.id) == null)
                throw new Exception("404;Task not found");

            if (await statusRepository.GetStatusByID(assignmentDTO.status.id) == null)
                throw new Exception("404;Status not found");

            assignment.task_id = assignmentDTO.task.id;

            assignment.status_id = assignmentDTO.status.id;

            assignment.date = assignmentDTO.date;

            this._context.Entry(assignment).State = EntityState.Modified;
        }

        public async Task ChangeStatusAssignment(AssignmentDTO assignmentDTO, int memberId)
        {
            MemberRepository memberRepository = new MemberRepository(this._context);

            Member? member = await memberRepository.GetMemberByID(memberId);

            Assignment? assignment = await this.GetAssignmentByID(assignmentDTO.id);

            if (assignment == null)
                throw new Exception("404;Assignment not found");

            if (member == null || assignment.member_id != memberId)
                throw new Exception("403;You do not have permission");

            StatusRepository statusRepository = new StatusRepository(this._context);

            if (await statusRepository.GetStatusByID(assignmentDTO.status.id) == null)
                throw new Exception("404;Status not found");

            assignment.status_id = assignmentDTO.status.id;

            this._context.Entry(assignment).State = EntityState.Modified;
        }

        public async Task DeleteAssignment(int id, int adminId)
        {
            MemberRepository memberRepository = new MemberRepository(this._context);

            Member? member = await memberRepository.GetMemberByID(id);

            if (member == null || (member.role.name != "Admin" && member.role.name != "Owner"))
                throw new Exception("403;You do not have permission");

            Assignment? assignment = await this.GetAssignmentByID(id);

            if (assignment == null)
                throw new Exception("404;Assignment not found");

            if (assignment.status.name == "Done")
                throw new Exception("409;Cannot delete a done assignment");

            this._context.Remove(assignment);
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

        private bool Exists(int memberID, int taskID, DateTime date)
        {
            return this._context.Assignments
                .Where(a => a.member_id == memberID && a.task_id == taskID && a.date == date)
                .Any();
        }

        #endregion

    }
}

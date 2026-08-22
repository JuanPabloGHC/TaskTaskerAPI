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

        private readonly IMemberRepository _memberRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IStatusRepository _statusRepository;

        #endregion

        #region CONSTRUCTOR

        public AssignmentRepository(
            TaskTaskerContext context,
            IMemberRepository memberRepository,
            ITaskRepository taskRepository,
            IStatusRepository statusRepository)
        {
            this._context = context;
            this._memberRepository = memberRepository;
            this._taskRepository = taskRepository;
            this._statusRepository = statusRepository;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Assignment>> GetMemberAssignments(int memberID)
        {
            return await this._context.Assignments
                .Where(a => a.member_id == memberID)
                .Include(a => a.task)
                .Include(a => a.status)
                .Include(a => a.member).ThenInclude(m => m.person)
                .Include(a => a.member).ThenInclude(m => m.home)
                .Include(a => a.member).ThenInclude(m => m.role)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Assignment> items, int total)> GetMemberAssignmentsPaged(int memberID, bool undone, int page, int pageSize)
        {
            IQueryable<Assignment> query = this._context.Assignments
                .Where(a => a.member_id == memberID)
                .Include(a => a.task)
                .Include(a => a.status)
                .Include(a => a.member).ThenInclude(m => m.person)
                .Include(a => a.member).ThenInclude(m => m.home)
                .Include(a => a.member).ThenInclude(m => m.role);

            if (undone)
                query = query.Where(a => a.status.name != "Done");

            return await Paginate(query, page, pageSize);
        }

        public async Task<(IEnumerable<Assignment> items, int total)> GetHomeAssignmentsPaged(int homeID, int page, int pageSize)
        {
            IQueryable<Assignment> query = this._context.Assignments
                .Where(a => a.member.home_id == homeID)
                .Include(a => a.task)
                .Include(a => a.status)
                .Include(a => a.member).ThenInclude(m => m.person)
                .Include(a => a.member).ThenInclude(m => m.home)
                .Include(a => a.member).ThenInclude(m => m.role);

            return await Paginate(query, page, pageSize);
        }

        public async Task<Assignment?> GetAssignmentByID(int id)
        {
            return await this._context.Assignments
                .Where(a => a.id == id)
                .Include(a => a.member).ThenInclude(m => m.home)
                .Include(a => a.member).ThenInclude(m => m.role)
                .Include(a => a.member).ThenInclude(m => m.person)
                .Include(a => a.status)
                .Include(a => a.task)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAssignment(AssignmentDTO assignmentDTO)
        {
            if (this.Exists(assignmentDTO.member.id, assignmentDTO.task.id, assignmentDTO.date))
                throw new Exception("409;Assignment already exists");

            if (await this._memberRepository.GetMemberByID(assignmentDTO.member.id) == null)
                throw new Exception("404;Member not found");

            if (await this._taskRepository.GetTaskByID(assignmentDTO.task.id) == null)
                throw new Exception("404;Task not found");

            if (await this._statusRepository.GetStatusByID(assignmentDTO.status.id) == null)
                throw new Exception("404;Status not found");

            Assignment assignment = new Assignment(assignmentDTO);

            await this._context.Assignments.AddAsync(assignment);
        }

        public async Task UpdateAssignment(AssignmentDTO assignmentDTO)
        {
            Assignment? assignment = await this.GetAssignmentByID(assignmentDTO.id);

            if (assignment == null)
                throw new Exception("404;Assignment not found");

            if (this.Exists(assignmentDTO.member.id, assignmentDTO.task.id, assignmentDTO.date))
                throw new Exception("409;Assignment already exists");

            if (await this._taskRepository.GetTaskByID(assignmentDTO.task.id) == null)
                throw new Exception("404;Task not found");

            if (await this._statusRepository.GetStatusByID(assignmentDTO.status.id) == null)
                throw new Exception("404;Status not found");

            assignment.task_id = assignmentDTO.task.id;

            assignment.status_id = assignmentDTO.status.id;

            assignment.date = assignmentDTO.date;

            this._context.Entry(assignment).State = EntityState.Modified;
        }

        public async Task ChangeStatusAssignment(AssignmentDTO assignmentDTO)
        {
            Assignment? assignment = await this.GetAssignmentByID(assignmentDTO.id);

            if (assignment == null)
                throw new Exception("404;Assignment not found");

            if (await this._statusRepository.GetStatusByID(assignmentDTO.status.id) == null)
                throw new Exception("404;Status not found");

            assignment.status_id = assignmentDTO.status.id;

            this._context.Entry(assignment).State = EntityState.Modified;
        }

        public async Task ApproveAssignment(int assignmentId, int doneStatusId)
        {
            Assignment? assignment = await this.GetAssignmentByID(assignmentId);

            if (assignment == null)
                throw new Exception("404;Assignment not found");

            assignment.status_id = doneStatusId;

            this._context.Entry(assignment).State = EntityState.Modified;
        }

        public async Task DeleteAssignment(int id)
        {
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

        private static async Task<(IEnumerable<Assignment> items, int total)> Paginate(IQueryable<Assignment> query, int page, int pageSize)
        {
            int total = await query.CountAsync();

            List<Assignment> items = await query
                .OrderByDescending(a => a.date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        #endregion

    }
}

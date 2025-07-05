using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class MemberRepository : IMemberRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        #endregion

        #region CONSTRUCTOR

        public MemberRepository(TaskTaskerContext context)
        {
            this._context = context;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Member>> GetHomeMembers(int homeID)
        {
            return await this._context.Members
                .Where(m => m.home_id == homeID)
                .Include(m => m.person)
                .Include(m => m.home)
                .Include(m => m.role)
                .ToListAsync();
        }

        public async Task<Member?> GetMemberByID(int id)
        {
            return await this._context.Members
                .Where(m => m.id == id)
                .Include(m => m.person)
                .Include(m => m.home)
                .Include(m => m.role)
                .FirstOrDefaultAsync();
        }

        public async Task CreateMember(MemberDTO memberDTO)
        {
            if (this.Exists(memberDTO.person.id, memberDTO.home.id))
                throw new Exception("409;Member already exists");

            Member member = new Member(memberDTO);

            await this._context.AddAsync(member);
        }

        public async Task UpdateMember(MemberDTO memberDTO)
        {
            Member? member = await this.GetMemberByID(memberDTO.id);

            if (member == null)
                throw new Exception("404;Member not found");

            member.role_id = memberDTO.role.id;

            this._context.Entry(member).State = EntityState.Modified;
        }

        public async Task DeleteMember(int id)
        {
            Member? member = await this.GetMemberByID(id);

            if (member == null)
                throw new Exception("404;Member not found");

            this._context.Remove(member);
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

        private bool Exists(int personID, int homeID)
        {
            return this._context.Members
                .Where(m => m.person_id == personID && m.home_id == homeID)
                .Any();
        }

        #endregion

    }
}

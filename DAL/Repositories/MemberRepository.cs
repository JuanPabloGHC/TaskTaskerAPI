using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Repositories
{
    public class MemberRepository : IMemberRepository, IDisposable
    {
        #region DATA MEMBERS

        private TaskTaskerContext _context;

        private bool disposed = false;

        private readonly IPersonRepository _personRepository;
        private readonly IHomeRepository _homeRepository;
        private readonly IRoleRepository _roleRepository;

        #endregion

        #region CONSTRUCTOR

        public MemberRepository(
            TaskTaskerContext context,
            IPersonRepository personRepository,
            IHomeRepository homeRepository,
            IRoleRepository roleRepository)
        {
            this._context = context;
            this._personRepository = personRepository;
            this._homeRepository = homeRepository;
            this._roleRepository = roleRepository;
        }

        #endregion

        #region PUBLIC METHODS

        public async Task<IEnumerable<Member>> GetMemberHomes(int personID)
        {
            return await this._context.Members
                .Include(m => m.home)
                .Include(m => m.person)
                .Include(m => m.role)
                .Where(m => m.person_id == personID)
                .ToListAsync();
        }

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

        public async Task<Member?> GetMemberByPersonAndHome(int personId, int homeId)
        {
            return await this._context.Members
                .Where(m => m.person_id == personId && m.home_id == homeId)
                .Include(m => m.person)
                .Include(m => m.home)
                .Include(m => m.role)
                .FirstOrDefaultAsync();
        }

        public async Task CreateMember(MemberDTO memberDTO)
        {
            if (this.Exists(memberDTO.person.id, memberDTO.home.id))
                throw new Exception("409;Member already exists");

            if (await this._personRepository.GetPersonByID(memberDTO.person.id) == null)
                throw new Exception("404;Person not found");

            if (await this._homeRepository.GetHomeByID(memberDTO.home.id) == null)
                throw new Exception("404;Home not found");

            if (await this._roleRepository.GetRoleByID(memberDTO.role.id) == null)
                throw new Exception("404;Role not found");

            Member member = new Member(memberDTO);

            await this._context.AddAsync(member);
        }

        public async Task<Member> CreateMemberByPhone(int homeId, string phone, int roleId)
        {
            Validate.Required("Phone", phone);

            Person? person = await this._personRepository.GetPersonByPhone(phone);

            if (person == null)
                throw new Exception("404;No account exists with that phone number");

            Home? home = await this._homeRepository.GetHomeByID(homeId);

            if (home == null)
                throw new Exception("404;Home not found");

            Role? role = await this._roleRepository.GetRoleByID(roleId);

            if (role == null)
                throw new Exception("404;Role not found");

            if (this.Exists(person.id, homeId))
                throw new Exception("409;This person is already a member of this home");

            // Linking the tracked entities lets EF fill the FKs and returns a fully
            // populated member for the response DTO without an extra query.
            Member member = new Member
            {
                person = person,
                home = home,
                role = role
            };

            await this._context.AddAsync(member);

            return member;
        }

        public async Task UpdateMember(MemberDTO memberDTO)
        {
            Member? member = await this.GetMemberByID(memberDTO.id);

            if (member == null)
                throw new Exception("404;Member not found");

            if (await this._roleRepository.GetRoleByID(memberDTO.role.id) == null)
                throw new Exception("404;Role not found");

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

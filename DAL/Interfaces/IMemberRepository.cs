using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IMemberRepository : IDisposable
    {
        Task<IEnumerable<Member>> GetMemberHomes(int personID);
        Task<IEnumerable<Member>> GetHomeMembers(int homeID);
        Task<Member?> GetMemberByID(int id);
        Task<Member?> GetMemberByPersonAndHome(int personId, int homeId);
        Task CreateMember(MemberDTO memberDTO);
        Task<Member> CreateMemberByPhone(int homeId, string phone, int roleId);
        Task UpdateMember(MemberDTO memberDTO);
        Task DeleteMember(int id);
        Task SaveChanges();
    }
}

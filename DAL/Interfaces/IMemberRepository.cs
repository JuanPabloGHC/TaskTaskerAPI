using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface IMemberRepository
    {
        Task<IEnumerable<Member>> GetHomeMembers(int homeID);
        Task<Member> GetMemberByID(int personID, int homeID);
        Task CreateMember(MemberDTO memberDTO);
        Task UpdateMember(MemberDTO memberDTO);
        Task DeleteMember(int id);
    }
}

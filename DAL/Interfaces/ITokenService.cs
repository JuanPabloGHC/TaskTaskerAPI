using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(Person person);
        string GenerateAdminAccessToken(AdminUser admin);
        RefreshToken GenerateRefreshToken(int personId);
    }

}

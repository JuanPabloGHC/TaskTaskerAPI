using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(Person person);
        RefreshToken GenerateRefreshToken(int personId);
    }

}

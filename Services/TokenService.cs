using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;

namespace TaskTaskerAPI.Services
{
    public class TokenService : ITokenService
    {
        #region DATA MEMBERS

        private readonly IConfiguration _configuration;

        #endregion

        #region CONSTRUCTOR

        public TokenService(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        #endregion

        #region PUBLIC METHODS

        public string GenerateAccessToken(Person person)
        {
            IConfigurationSection jwt = this._configuration.GetSection("Jwt");

            SymmetricSecurityKey key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            SigningCredentials credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256
            );

            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, person.id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, person.name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["AccessTokenMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(int personId)
        {
            IConfigurationSection jwt = this._configuration.GetSection("Jwt");

            byte[] randomBytes = RandomNumberGenerator.GetBytes(64);

            return new RefreshToken
            {
                person_id = personId,
                token = Convert.ToBase64String(randomBytes),
                expires_at = DateTime.UtcNow.AddDays(double.Parse(jwt["RefreshTokenDays"]!)),
                is_revoked = false,
                created_at = DateTime.UtcNow
            };
        }

        #endregion

    }
}

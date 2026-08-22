using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        #region DATA MEMBERS

        private readonly IPersonRepository personRepository;
        private readonly IRefreshTokenRepository refreshTokenRepository;
        private readonly ITokenService tokenService;
        private readonly IPasswordHasher<Person> passwordHasher;

        #endregion

        #region CONSTRUCTOR

        public AuthController(
            IPersonRepository personRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IPasswordHasher<Person> passwordHasher)
        {
            this.personRepository = personRepository;
            this.refreshTokenRepository = refreshTokenRepository;
            this.tokenService = tokenService;
            this.passwordHasher = passwordHasher;
        }

        #endregion

        #region ENDPOINTS

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            try
            {
                Person? person = await this.personRepository.GetPersonByName(loginRequest.name);

                if (person == null)
                    throw new Exception("401;Invalid credentials");

                PasswordVerificationResult result = this.passwordHasher.VerifyHashedPassword(
                    person, person.password, loginRequest.password
                );

                if (result == PasswordVerificationResult.Failed)
                    throw new Exception("401;Invalid credentials");

                AuthResponseDTO authResponse = await this.IssueTokens(person);

                return Ok(new ApiResponse<AuthResponseDTO>
                {
                    StatusCode = 200,
                    Message = "Login successful",
                    Data = authResponse
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO refreshRequest)
        {
            try
            {
                RefreshToken? stored = await this.refreshTokenRepository.GetByToken(refreshRequest.refreshToken);

                if (stored == null || stored.is_revoked || stored.expires_at < DateTime.UtcNow)
                    throw new Exception("401;Invalid or expired refresh token");

                Person? person = await this.personRepository.GetPersonByID(stored.person_id);

                if (person == null)
                    throw new Exception("401;Invalid refresh token");

                // Rotation: the used refresh token is revoked and a new pair is issued.
                this.refreshTokenRepository.RevokeToken(stored);

                AuthResponseDTO authResponse = await this.IssueTokens(person);

                return Ok(new ApiResponse<AuthResponseDTO>
                {
                    StatusCode = 200,
                    Message = "Token refreshed",
                    Data = authResponse
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDTO refreshRequest)
        {
            try
            {
                RefreshToken? stored = await this.refreshTokenRepository.GetByToken(refreshRequest.refreshToken);

                if (stored != null && !stored.is_revoked)
                {
                    this.refreshTokenRepository.RevokeToken(stored);
                    await this.refreshTokenRepository.SaveChanges();
                }

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Logged out",
                    Data = string.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        #endregion

        #region PRIVATE METHODS

        private async Task<AuthResponseDTO> IssueTokens(Person person)
        {
            string accessToken = this.tokenService.GenerateAccessToken(person);

            RefreshToken refreshToken = this.tokenService.GenerateRefreshToken(person.id);

            await this.refreshTokenRepository.AddRefreshToken(refreshToken);

            await this.refreshTokenRepository.SaveChanges();

            return new AuthResponseDTO
            {
                accessToken = accessToken,
                refreshToken = refreshToken.token,
                person = new PersonDTO(person)
            };
        }

        private IActionResult CatchReturn(Exception ex)
        {
            string[] error = ex.Message.Split(';');

            if (error.Length == 1)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = String.Empty
                });
            }

            return StatusCode(Convert.ToInt32(error[0]), new ApiResponse<string>
            {
                StatusCode = Convert.ToInt32(error[0]),
                Message = error[1],
                Data = String.Empty
            });
        }

        #endregion

    }
}

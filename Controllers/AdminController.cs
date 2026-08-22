using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        #region DATA MEMBERS

        private readonly IAdminUserRepository adminUserRepository;
        private readonly ITokenService tokenService;
        private readonly IPasswordHasher<AdminUser> passwordHasher;

        #endregion

        #region CONSTRUCTOR

        public AdminController(
            IAdminUserRepository adminUserRepository,
            ITokenService tokenService,
            IPasswordHasher<AdminUser> passwordHasher)
        {
            this.adminUserRepository = adminUserRepository;
            this.tokenService = tokenService;
            this.passwordHasher = passwordHasher;
        }

        #endregion

        #region ENDPOINTS

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] AdminCredentialsDTO credentials)
        {
            try
            {
                AdminUser? admin = await this.adminUserRepository.GetByUsername(credentials.username);

                if (admin == null)
                    throw new Exception("401;Invalid credentials");

                PasswordVerificationResult result = this.passwordHasher.VerifyHashedPassword(
                    admin, admin.password, credentials.password
                );

                if (result == PasswordVerificationResult.Failed)
                    throw new Exception("401;Invalid credentials");

                string accessToken = this.tokenService.GenerateAdminAccessToken(admin);

                return Ok(new ApiResponse<AdminAuthResponseDTO>
                {
                    StatusCode = 200,
                    Message = "Login successful",
                    Data = new AdminAuthResponseDTO
                    {
                        accessToken = accessToken,
                        username = admin.username
                    }
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        // Bootstrap-friendly: the very first admin can be created without a token
        // (the table is empty); after that, only an authenticated platform admin can.
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] AdminCredentialsDTO credentials)
        {
            try
            {
                bool adminsExist = await this.adminUserRepository.AnyAdmins();

                if (adminsExist && !User.IsInRole("platform_admin"))
                    throw new Exception("403;Only a platform admin can create new admins");

                if (string.IsNullOrWhiteSpace(credentials.username) || string.IsNullOrWhiteSpace(credentials.password))
                    throw new Exception("400;Username and password are required");

                AdminUser admin = await this.adminUserRepository.CreateAdmin(credentials.username, credentials.password);

                await this.adminUserRepository.SaveChanges();

                return Created("", new ApiResponse<AdminAuthResponseDTO>
                {
                    StatusCode = 201,
                    Message = "Admin created successfully",
                    Data = new AdminAuthResponseDTO
                    {
                        accessToken = string.Empty,
                        username = admin.username
                    }
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize(Roles = "platform_admin")]
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                await this.adminUserRepository.DeleteAdmin(id);

                await this.adminUserRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "Admin deleted successfully",
                    Data = String.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        #endregion

        #region PRIVATE METHODS

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

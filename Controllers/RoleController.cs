using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/role")]
    public class RoleController : Controller
    {
        #region DATA MEMBERS

        private IRoleRepository roleRepository;

        #endregion

        #region CONSTRUCTOR

        public RoleController(IRoleRepository roleRepository)
        {
            this.roleRepository = roleRepository;
        }

        #endregion

        #region ENDPOINTS

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<Role> roles = (List<Role>)await this.roleRepository.GetAllRoles();

                List<RoleDTO> rolesDTO = new List<RoleDTO>();

                foreach (Role role in roles)
                {
                    rolesDTO.Add(new RoleDTO(role));
                }

                return Ok(new ApiResponse<List<RoleDTO>>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = rolesDTO
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] RoleDTO roleDTO)
        {
            try
            {
                await this.roleRepository.CreateRole(roleDTO);

                await this.roleRepository.SaveChanges();

                return Created("", new ApiResponse<string>
                {
                    StatusCode = 201,
                    Message = "Role created successfully",
                    Data = String.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] RoleDTO roleDTO, [FromRoute] int id)
        {
            try
            {
                if (id != roleDTO.id)
                    throw new Exception("400;ID does not match");

                await this.roleRepository.UpdateRole(roleDTO);

                await this.roleRepository.SaveChanges();

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Role modified successfully",
                    Data = String.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                await this.roleRepository.DeleteRole(id);

                await this.roleRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "Role deleted successfully",
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

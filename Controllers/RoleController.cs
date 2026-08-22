using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/role")]
    public class RoleController : ControllerBase
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

        [Authorize]
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll()
        {
            List<Role> roles = (List<Role>)await this.roleRepository.GetAllRoles();

            return Ok(new ApiResponse<List<RoleDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = roles.ConvertAll(r => new RoleDTO(r))
            });
        }

        [Authorize]
        [HttpGet]
        [Route("get/{id:int}")]
        public async Task<IActionResult> GetByID([FromRoute] int id)
        {
            Role? role = await this.roleRepository.GetRoleByID(id);

            if (role == null)
                throw new ApiException(404, "Role not found");

            return Ok(new ApiResponse<RoleDTO>
            {
                StatusCode = 200,
                Message = "",
                Data = new RoleDTO(role)
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] RoleDTO roleDTO)
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

        [Authorize(Roles = "platform_admin")]
        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] RoleDTO roleDTO, [FromRoute] int id)
        {
            if (id != roleDTO.id)
                throw new ApiException(400, "ID does not match");

            await this.roleRepository.UpdateRole(roleDTO);

            await this.roleRepository.SaveChanges();

            return Ok(new ApiResponse<string>
            {
                StatusCode = 200,
                Message = "Role modified successfully",
                Data = String.Empty
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
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

        #endregion

    }
}

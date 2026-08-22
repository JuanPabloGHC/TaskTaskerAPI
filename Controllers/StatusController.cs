using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class StatusController : ControllerBase
    {
        #region DATA MEMBERS

        private IStatusRepository statusRepository;

        #endregion

        #region CONSTRUCTOR

        public StatusController(IStatusRepository statusRepository)
        {
            this.statusRepository = statusRepository;
        }

        #endregion

        #region ENDPOINTS

        [Authorize]
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll()
        {
            List<Status> statuses = (List<Status>)await this.statusRepository.GetAllStatuses();

            return Ok(new ApiResponse<List<StatusDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = statuses.ConvertAll(s => new StatusDTO(s))
            });
        }

        [Authorize]
        [HttpGet]
        [Route("get/{id:int}")]
        public async Task<IActionResult> GetByID([FromRoute] int id)
        {
            Status? status = await this.statusRepository.GetStatusByID(id);

            if (status == null)
                throw new ApiException(404, "Status not found");

            return Ok(new ApiResponse<StatusDTO>
            {
                StatusCode = 200,
                Message = "",
                Data = new StatusDTO(status)
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] StatusDTO statusDTO)
        {
            await this.statusRepository.CreateStatus(statusDTO);

            await this.statusRepository.SaveChanges();

            return Created("", new ApiResponse<string>
            {
                StatusCode = 201,
                Message = "Status created successfully",
                Data = String.Empty
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] StatusDTO statusDTO, [FromRoute] int id)
        {
            if (id != statusDTO.id)
                throw new ApiException(400, "ID does not match");

            await this.statusRepository.UpdateStatus(statusDTO);

            await this.statusRepository.SaveChanges();

            return Ok(new ApiResponse<string>
            {
                StatusCode = 200,
                Message = "Status modified successfully",
                Data = String.Empty
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await this.statusRepository.DeleteStatus(id);

            await this.statusRepository.SaveChanges();

            return StatusCode(204, new ApiResponse<string>
            {
                StatusCode = 204,
                Message = "Status deleted successfully",
                Data = String.Empty
            });
        }

        #endregion

    }
}

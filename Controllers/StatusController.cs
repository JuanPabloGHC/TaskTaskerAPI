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
    public class StatusController : Controller
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
            try
            {
                List<Status> statuses = (List<Status>)await this.statusRepository.GetAllStatuses();

                List<StatusDTO> statusesDTO = new List<StatusDTO>();

                foreach (Status status in statuses)
                {
                    statusesDTO.Add(new StatusDTO(status));
                }

                return Ok(new ApiResponse<List<StatusDTO>>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = statusesDTO
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("get/{id:int}")]
        public async Task<IActionResult> GetByID([FromRoute] int id)
        {
            try
            {
                Status? status = await this.statusRepository.GetStatusByID(id);

                if (status == null)
                    throw new Exception("404;Status not found");

                return Ok(new ApiResponse<StatusDTO>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = new StatusDTO(status)
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize(Roles = "platform_admin")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] StatusDTO statusDTO)
        {
            try
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
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize(Roles = "platform_admin")]
        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] StatusDTO statusDTO, [FromRoute] int id)
        {
            try
            {
                if (id != statusDTO.id)
                    throw new Exception("400;ID does not match");

                await this.statusRepository.UpdateStatus(statusDTO);

                await this.statusRepository.SaveChanges();

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Status modified successfully",
                    Data = String.Empty
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
                await this.statusRepository.DeleteStatus(id);

                await this.statusRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "Status deleted successfully",
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

        public IActionResult CatchReturn(Exception ex)
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

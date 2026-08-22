using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/task")]
    public class TaskController : Controller
    {
        #region DATA MEMBERS

        private ITaskRepository taskRepository;

        #endregion

        #region CONSTRUCTOR

        public TaskController(ITaskRepository taskRepository)
        {
            this.taskRepository = taskRepository;
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
                List<DAL.Entities.Task> tasks = (List<DAL.Entities.Task>)await this.taskRepository.GetAllTasks();

                List<TaskDTO> tasksDTO = new List<TaskDTO>();

                foreach (DAL.Entities.Task task in tasks)
                {
                    tasksDTO.Add(new TaskDTO(task));
                }

                return Ok(new ApiResponse<List<TaskDTO>>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = tasksDTO
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
                DAL.Entities.Task? task = await this.taskRepository.GetTaskByID(id);

                if (task == null)
                    throw new Exception("404;Task not found");

                return Ok(new ApiResponse<TaskDTO>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = new TaskDTO(task)
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
        public async Task<IActionResult> Create([FromBody] TaskDTO taskDTO)
        {
            try
            {
                await this.taskRepository.CreateTask(taskDTO);

                await this.taskRepository.SaveChanges();

                return Created("", new ApiResponse<string>
                {
                    StatusCode = 201,
                    Message = "Task created successfully",
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
        public async Task<IActionResult> Update([FromBody] TaskDTO taskDTO, [FromRoute] int id)
        {
            try
            {
                if (id != taskDTO.id)
                    throw new Exception("400;ID does not match");

                await this.taskRepository.UpdateTask(taskDTO);

                await this.taskRepository.SaveChanges();

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Task modified successfully",
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
                await this.taskRepository.DeleteTask(id);

                await this.taskRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "Task deleted successfully",
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

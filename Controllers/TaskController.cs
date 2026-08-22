using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/task")]
    public class TaskController : ControllerBase
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
            List<DAL.Entities.Task> tasks = (List<DAL.Entities.Task>)await this.taskRepository.GetAllTasks();

            return Ok(new ApiResponse<List<TaskDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = tasks.ConvertAll(t => new TaskDTO(t))
            });
        }

        [Authorize]
        [HttpGet]
        [Route("get/{id:int}")]
        public async Task<IActionResult> GetByID([FromRoute] int id)
        {
            DAL.Entities.Task? task = await this.taskRepository.GetTaskByID(id);

            if (task == null)
                throw new ApiException(404, "Task not found");

            return Ok(new ApiResponse<TaskDTO>
            {
                StatusCode = 200,
                Message = "",
                Data = new TaskDTO(task)
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] TaskDTO taskDTO)
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

        [Authorize(Roles = "platform_admin")]
        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] TaskDTO taskDTO, [FromRoute] int id)
        {
            if (id != taskDTO.id)
                throw new ApiException(400, "ID does not match");

            await this.taskRepository.UpdateTask(taskDTO);

            await this.taskRepository.SaveChanges();

            return Ok(new ApiResponse<string>
            {
                StatusCode = 200,
                Message = "Task modified successfully",
                Data = String.Empty
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
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

        #endregion

    }
}

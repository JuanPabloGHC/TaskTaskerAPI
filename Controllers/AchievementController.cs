using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/achievement")]
    public class AchievementController : ControllerBase
    {
        #region DATA MEMBERS

        private IAchievementRepository achievementRepository;

        #endregion

        #region CONSTRUCTOR

        public AchievementController(IAchievementRepository achievementRepository)
        {
            this.achievementRepository = achievementRepository;
        }

        #endregion

        #region ENDPOINTS

        [Authorize]
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll()
        {
            List<Achievement> achievements = (List<Achievement>)await this.achievementRepository.GetAllAchievements();

            return Ok(new ApiResponse<List<AchievementDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = achievements.ConvertAll(a => new AchievementDTO(a))
            });
        }

        [Authorize]
        [HttpGet]
        [Route("get/{id:int}")]
        public async Task<IActionResult> GetByID([FromRoute] int id)
        {
            Achievement? achievement = await this.achievementRepository.GetAchievementByID(id);

            if (achievement == null)
                throw new ApiException(404, "Achievement not found");

            return Ok(new ApiResponse<AchievementDTO>
            {
                StatusCode = 200,
                Message = "",
                Data = new AchievementDTO(achievement)
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] AchievementDTO achievementDTO)
        {
            await this.achievementRepository.CreateAchievement(achievementDTO);

            await this.achievementRepository.SaveChanges();

            return Created("", new ApiResponse<string>
            {
                StatusCode = 201,
                Message = "Achievement created successfully",
                Data = String.Empty
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] AchievementDTO achievementDTO, [FromRoute] int id)
        {
            if (id != achievementDTO.id)
                throw new ApiException(400, "ID does not match");

            await this.achievementRepository.UpdateAchievement(achievementDTO);

            await this.achievementRepository.SaveChanges();

            return Ok(new ApiResponse<string>
            {
                StatusCode = 200,
                Message = "Achievement modified successfully",
                Data = String.Empty
            });
        }

        [Authorize(Roles = "platform_admin")]
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await this.achievementRepository.DeleteAchievement(id);

            await this.achievementRepository.SaveChanges();

            return StatusCode(204, new ApiResponse<string>
            {
                StatusCode = 204,
                Message = "Achievement deleted succesfully",
                Data = String.Empty
            });
        }

        #endregion

    }
}

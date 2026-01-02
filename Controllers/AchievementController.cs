using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/achievement")]
    public class AchievementController : Controller
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

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<Achievement> achievements = (List<Achievement>)await this.achievementRepository.GetAllAchievements();

                List<AchievementDTO> achievementsDTO = new List<AchievementDTO>();

                foreach (Achievement achievement in achievements)
                {
                    achievementsDTO.Add(new AchievementDTO(achievement));
                }

                return Ok(new ApiResponse<List<AchievementDTO>>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = achievementsDTO
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] AchievementDTO achievementDTO)
        {
            try
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
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] AchievementDTO achievementDTO, [FromRoute] int id)
        {
            try
            {
                if (id != achievementDTO.id)
                    throw new Exception("400;ID does not match");

                await this.achievementRepository.UpdateAchievement(achievementDTO);

                await this.achievementRepository.SaveChanges();

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Achievement modified successfully",
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
                await this.achievementRepository.DeleteAchievement(id);

                await this.achievementRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "Achievement deleted succesfully",
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

using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.DAL.Repositories;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/home")]
    public class HomeController : Controller
    {
        #region CLASSES

        public class NewHome
        {
            public HomeDTO homeDTO { get; set; }
            public PersonDTO personDTO { get; set; }

            public NewHome()
            {
                this.homeDTO = new HomeDTO();
                this.personDTO = new PersonDTO();
            }
        }

        #endregion

        #region DATA MEMBERS

        private IHomeRepository homeRepository;

        #endregion

        #region CONSTRUCTOR

        public HomeController(IHomeRepository homeRepository)
        {
            this.homeRepository = homeRepository;
        }

        #endregion

        #region ENDPOINTS

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] NewHome newHome)
        {
            try
            {
                Home home = await this.homeRepository.CreateHome(newHome.homeDTO, newHome.personDTO);

                await this.homeRepository.SaveChanges();

                newHome.homeDTO = new HomeDTO(home);

                return Created("", new ApiResponse<HomeDTO>
                {
                    StatusCode = 201,
                    Message = "Home created successfully",
                    Data = newHome.homeDTO
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] HomeDTO homeDTO, [FromRoute] int id)
        {
            try
            {
                if (id != homeDTO.id)
                    throw new Exception("400;ID does not match");

                Home home = await this.homeRepository.UpdateHome(homeDTO);

                await this.homeRepository.SaveChanges();

                homeDTO = new HomeDTO(home);

                return Ok(new ApiResponse<HomeDTO>
                {
                    StatusCode = 200,
                    Message = "Home modified successfully",
                    Data = homeDTO
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
                await this.homeRepository.DeleteHome(id);

                await this.homeRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "Home deleted successfully",
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/person")]
    public class PersonController : AppControllerBase
    {
        #region DATA MEMBERS

        private IPersonRepository personRepository;

        #endregion

        #region CONSTRUCTOR

        public PersonController(IPersonRepository personRepository)
        {
            this.personRepository = personRepository;
        }

        #endregion

        #region ENDPOINTS

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Signup([FromBody] PersonDTO personDTO)
        {
            try
            {
                Person person = await this.personRepository.CreatePerson(personDTO);

                await this.personRepository.SaveChanges();

                personDTO = new PersonDTO(person);

                return Created("", new ApiResponse<PersonDTO>
                {
                    StatusCode = 201,
                    Message = "User created successfully",
                    Data = personDTO
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("me")]
        public async Task<IActionResult> Me()
        {
            try
            {
                Person? person = await this.personRepository.GetPersonByID(this.GetPersonId());

                if (person == null)
                    throw new Exception("404;User not found");

                return Ok(new ApiResponse<PersonDTO>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = new PersonDTO(person)
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] PersonDTO personDTO, [FromRoute] int id)
        {
            try
            {
                if (id != this.GetPersonId())
                    throw new Exception("403;You can only update your own account");

                if (id != personDTO.id)
                    throw new Exception("400;ID does not match");

                Person person = await this.personRepository.UpdatePerson(personDTO);

                await this.personRepository.SaveChanges();

                personDTO = new PersonDTO(person);

                return Ok(new ApiResponse<PersonDTO>
                {
                    StatusCode = 200,
                    Message = "User modified successfully",
                    Data = personDTO
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                if (id != this.GetPersonId())
                    throw new Exception("403;You can only delete your own account");

                await this.personRepository.DeletePerson(id);

                await this.personRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "User deleted successfully",
                    Data = String.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        #endregion

    }
}

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
            Person person = await this.personRepository.CreatePerson(personDTO);

            await this.personRepository.SaveChanges();

            return Created("", new ApiResponse<PersonDTO>
            {
                StatusCode = 201,
                Message = "User created successfully",
                Data = new PersonDTO(person)
            });
        }

        [Authorize]
        [HttpGet]
        [Route("me")]
        public async Task<IActionResult> Me()
        {
            Person? person = await this.personRepository.GetPersonByID(this.GetPersonId());

            if (person == null)
                throw new ApiException(404, "User not found");

            return Ok(new ApiResponse<PersonDTO>
            {
                StatusCode = 200,
                Message = "",
                Data = new PersonDTO(person)
            });
        }

        [Authorize]
        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] PersonDTO personDTO, [FromRoute] int id)
        {
            if (id != this.GetPersonId())
                throw new ApiException(403, "You can only update your own account");

            if (id != personDTO.id)
                throw new ApiException(400, "ID does not match");

            Person person = await this.personRepository.UpdatePerson(personDTO);

            await this.personRepository.SaveChanges();

            return Ok(new ApiResponse<PersonDTO>
            {
                StatusCode = 200,
                Message = "User modified successfully",
                Data = new PersonDTO(person)
            });
        }

        [Authorize]
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id != this.GetPersonId())
                throw new ApiException(403, "You can only delete your own account");

            await this.personRepository.DeletePerson(id);

            await this.personRepository.SaveChanges();

            return StatusCode(204, new ApiResponse<string>
            {
                StatusCode = 204,
                Message = "User deleted successfully",
                Data = String.Empty
            });
        }

        #endregion

    }
}

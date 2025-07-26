using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/person")]
    public class PersonController : Controller
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
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] PersonDTO personDTO)
        {
            try
            {
                Person? person = await this.personRepository.GetPersonByNameAndPassword(personDTO.name, personDTO.password);

                if (person == null)
                    throw new Exception("404;User not found");

                personDTO = new PersonDTO(person);

                return Ok(new ApiResponse<PersonDTO>
                {
                    StatusCode = 200,
                    Message = "Verified user",
                    Data = personDTO
                });

            }
            catch (Exception ex)
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
        }

        [HttpPatch]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] PersonDTO personDTO)
        {
            try
            {
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
        }

        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
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
        }

        #endregion

    }
}

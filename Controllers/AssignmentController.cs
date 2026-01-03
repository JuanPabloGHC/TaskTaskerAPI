using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/assignment")]
    public class AssignmentController : Controller
    {
        #region DATA MEMBERS

        private IAssignmentRepository assignmentRepository;

        #endregion

        #region CONSTRUCTOR

        public AssignmentController(IAssignmentRepository assignmentRepository)
        {
            this.assignmentRepository = assignmentRepository;
        }

        #endregion

        #region ENDPOINTS

        [HttpGet]
        [Route("get-all/member/{memberId:int}")]
        public async Task<IActionResult> GetAllFromMember([FromRoute] int memberId, [FromQuery] bool undone = false)
        {
            try
            {
                List<Assignment> assignments = (List<Assignment>)(
                    undone ? 
                        await this.assignmentRepository.GetUndoneMemberAssignments(memberId)
                    :
                        await this.assignmentRepository.GetMemberAssignments(memberId)
                );

                List<AssignmentDTO> assignmentDTOs = new List<AssignmentDTO>();

                foreach (Assignment assignment in assignments)
                {
                    assignmentDTOs.Add(new AssignmentDTO(assignment));
                }

                return Ok(new ApiResponse<List<AssignmentDTO>>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = assignmentDTOs
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPost]
        [Route("create/{adminId:int}")]
        public async Task<IActionResult> Create([FromBody] AssignmentDTO assignmentDTO, [FromRoute] int adminId)
        {
            try
            {
                await this.assignmentRepository.CreateAssignment(assignmentDTO, adminId);

                await this.assignmentRepository.SaveChanges();

                return Created("", new ApiResponse<string>
                {
                    StatusCode = 201,
                    Message = "Assignment created successfully.",
                    Data = string.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPatch]
        [Route("change-status/{assignmentId:int}/{memberId:int}")]
        public async Task<IActionResult> ChangeStatus([FromRoute] int assignmentId, [FromRoute] int memberId, [FromBody] AssignmentDTO assignmentDTO)
        {
            try
            {
                if (assignmentId != assignmentDTO.id)
                    throw new Exception("400;ID does not match");

                await this.assignmentRepository.UpdateAssignment(assignmentDTO, memberId);

                await this.assignmentRepository.SaveChanges();

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Assignment status updated successfully.",
                    Data = string.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpDelete]
        [Route("delete/{assignmentId:int}/{adminId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int assignmentId, [FromRoute] int adminId)
        {
            try
            {
                await this.assignmentRepository.DeleteAssignment(assignmentId, adminId);

                await this.assignmentRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "Assignment deleted successfully",
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

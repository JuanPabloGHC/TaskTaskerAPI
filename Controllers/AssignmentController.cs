using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/assignment")]
    public class AssignmentController : AppControllerBase
    {
        #region DATA MEMBERS

        private IAssignmentRepository assignmentRepository;
        private IMemberRepository memberRepository;

        #endregion

        #region CONSTRUCTOR

        public AssignmentController(IAssignmentRepository assignmentRepository, IMemberRepository memberRepository)
        {
            this.assignmentRepository = assignmentRepository;
            this.memberRepository = memberRepository;
        }

        #endregion

        #region ENDPOINTS

        [Authorize]
        [HttpGet]
        [Route("get-all/member/{memberId:int}")]
        public async Task<IActionResult> GetAllFromMember([FromRoute] int memberId, [FromQuery] bool undone = false)
        {
            try
            {
                Member? target = await this.memberRepository.GetMemberByID(memberId);

                if (target == null)
                    throw new Exception("404;Member not found");

                // Members see their own assignments; Owners/Admins can see any member's.
                if (target.person_id != this.GetPersonId())
                {
                    Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), target.home_id);

                    RequireRole(caller, "Owner", "Admin");
                }

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

        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] AssignmentDTO assignmentDTO)
        {
            try
            {
                Member? targetMember = await this.memberRepository.GetMemberByID(assignmentDTO.member.id);

                if (targetMember == null)
                    throw new Exception("404;Member not found");

                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), targetMember.home_id);

                RequireRole(caller, "Owner", "Admin");

                await this.assignmentRepository.CreateAssignment(assignmentDTO);

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

        [Authorize]
        [HttpPatch]
        [Route("update/{assignmentId:int}")]
        public async Task<IActionResult> Update([FromRoute] int assignmentId, [FromBody] AssignmentDTO assignmentDTO)
        {
            try
            {
                if (assignmentId != assignmentDTO.id)
                    throw new Exception("400;ID does not match");

                Assignment? assignment = await this.assignmentRepository.GetAssignmentByID(assignmentId);

                if (assignment == null)
                    throw new Exception("404;Assignment not found");

                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), assignment.member.home_id);

                RequireRole(caller, "Owner", "Admin");

                await this.assignmentRepository.UpdateAssignment(assignmentDTO);

                await this.assignmentRepository.SaveChanges();

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Assignment updated successfully.",
                    Data = string.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("change-status/{assignmentId:int}")]
        public async Task<IActionResult> ChangeStatus([FromRoute] int assignmentId, [FromBody] AssignmentDTO assignmentDTO)
        {
            try
            {
                if (assignmentId != assignmentDTO.id)
                    throw new Exception("400;ID does not match");

                Assignment? assignment = await this.assignmentRepository.GetAssignmentByID(assignmentId);

                if (assignment == null)
                    throw new Exception("404;Assignment not found");

                // Only the assignee can change the status of their own assignment.
                if (assignment.member.person_id != this.GetPersonId())
                    throw new Exception("403;You do not have permission");

                await this.assignmentRepository.ChangeStatusAssignment(assignmentDTO);

                await this.assignmentRepository.SaveChanges();

                if (assignmentDTO.status.name == "Done")
                {
                    IAttainmentRepository attainmentRepository;
                    attainmentRepository = (IAttainmentRepository)HttpContext.RequestServices.GetService(typeof(IAttainmentRepository))!;

                    if (attainmentRepository == null)
                        throw new Exception("500;Internal server error");

                    List<Achievement> newAchievements = await attainmentRepository.ValidateAchievement(assignment.member_id, assignmentDTO.task.id);

                    if (newAchievements.Count > 0)
                    {
                        foreach (Achievement achievement in newAchievements)
                            await attainmentRepository.CreateAttainment(assignment.member_id, achievement.id);

                        await attainmentRepository.SaveChanges();
                    }

                    return Ok(new ApiResponse<List<AchievementDTO>>
                    {
                        StatusCode = 200,
                        Message = "Assignment status changed successfully.",
                        Data = newAchievements.ConvertAll(a => new AchievementDTO(a))
                    });
                }

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Assignment status changed successfully.",
                    Data = string.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("delete/{assignmentId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int assignmentId)
        {
            try
            {
                Assignment? assignment = await this.assignmentRepository.GetAssignmentByID(assignmentId);

                if (assignment == null)
                    throw new Exception("404;Assignment not found");

                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), assignment.member.home_id);

                RequireRole(caller, "Owner", "Admin");

                await this.assignmentRepository.DeleteAssignment(assignmentId);

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

    }
}

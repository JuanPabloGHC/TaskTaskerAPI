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
        private IStatusRepository statusRepository;
        private IAttainmentRepository attainmentRepository;

        #endregion

        #region CONSTRUCTOR

        public AssignmentController(
            IAssignmentRepository assignmentRepository,
            IMemberRepository memberRepository,
            IStatusRepository statusRepository,
            IAttainmentRepository attainmentRepository)
        {
            this.assignmentRepository = assignmentRepository;
            this.memberRepository = memberRepository;
            this.statusRepository = statusRepository;
            this.attainmentRepository = attainmentRepository;
        }

        #endregion

        #region ENDPOINTS

        [Authorize]
        [HttpGet]
        [Route("get-all/member/{memberId:int}")]
        public async Task<IActionResult> GetAllFromMember([FromRoute] int memberId, [FromQuery] bool undone = false)
        {
            Member? target = await this.memberRepository.GetMemberByID(memberId);

            if (target == null)
                throw new ApiException(404, "Member not found");

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

            return Ok(new ApiResponse<List<AssignmentDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = assignments.ConvertAll(a => new AssignmentDTO(a))
            });
        }

        [Authorize]
        [HttpGet]
        [Route("get-all/home/{homeId:int}")]
        public async Task<IActionResult> GetAllFromHome([FromRoute] int homeId)
        {
            Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), homeId);

            RequireMembership(caller);

            List<Assignment> assignments = (List<Assignment>)await this.assignmentRepository.GetHomeAssignments(homeId);

            return Ok(new ApiResponse<List<AssignmentDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = assignments.ConvertAll(a => new AssignmentDTO(a))
            });
        }

        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] AssignmentDTO assignmentDTO)
        {
            Member? targetMember = await this.memberRepository.GetMemberByID(assignmentDTO.member.id);

            if (targetMember == null)
                throw new ApiException(404, "Member not found");

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

        [Authorize]
        [HttpPatch]
        [Route("update/{assignmentId:int}")]
        public async Task<IActionResult> Update([FromRoute] int assignmentId, [FromBody] AssignmentDTO assignmentDTO)
        {
            if (assignmentId != assignmentDTO.id)
                throw new ApiException(400, "ID does not match");

            Assignment? assignment = await this.assignmentRepository.GetAssignmentByID(assignmentId);

            if (assignment == null)
                throw new ApiException(404, "Assignment not found");

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

        [Authorize]
        [HttpPatch]
        [Route("change-status/{assignmentId:int}")]
        public async Task<IActionResult> ChangeStatus([FromRoute] int assignmentId, [FromBody] AssignmentDTO assignmentDTO)
        {
            if (assignmentId != assignmentDTO.id)
                throw new ApiException(400, "ID does not match");

            Assignment? assignment = await this.assignmentRepository.GetAssignmentByID(assignmentId);

            if (assignment == null)
                throw new ApiException(404, "Assignment not found");

            // Only the assignee can change the status of their own assignment.
            if (assignment.member.person_id != this.GetPersonId())
                throw new ApiException(403, "You do not have permission");

            // "Done" is granted only by an Owner/Admin via the approval flow, so the
            // assignee cannot mark it done directly (they submit it for review instead).
            Status? requested = await this.statusRepository.GetStatusByID(assignmentDTO.status.id);

            if (requested != null && requested.name == "Done")
                throw new ApiException(403, "Only an Owner or Admin can approve a task as Done");

            await this.assignmentRepository.ChangeStatusAssignment(assignmentDTO);

            await this.assignmentRepository.SaveChanges();

            return Ok(new ApiResponse<string>
            {
                StatusCode = 200,
                Message = "Assignment status changed successfully.",
                Data = string.Empty
            });
        }

        [Authorize]
        [HttpPatch]
        [Route("approve/{assignmentId:int}")]
        public async Task<IActionResult> Approve([FromRoute] int assignmentId)
        {
            Assignment? assignment = await this.assignmentRepository.GetAssignmentByID(assignmentId);

            if (assignment == null)
                throw new ApiException(404, "Assignment not found");

            Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), assignment.member.home_id);

            RequireRole(caller, "Owner", "Admin");

            Status? done = await this.statusRepository.GetStatusByName("Done");

            if (done == null)
                throw new ApiException(500, "\"Done\" status is not configured");

            await this.assignmentRepository.ApproveAssignment(assignmentId, done.id);

            await this.assignmentRepository.SaveChanges();

            // Award any achievements the member has now earned for this task.
            List<Achievement> newAchievements = await this.attainmentRepository.ValidateAchievement(assignment.member_id, assignment.task_id);

            if (newAchievements.Count > 0)
            {
                foreach (Achievement achievement in newAchievements)
                    await this.attainmentRepository.CreateAttainment(assignment.member_id, achievement.id);

                await this.attainmentRepository.SaveChanges();
            }

            return Ok(new ApiResponse<List<AchievementDTO>>
            {
                StatusCode = 200,
                Message = "Assignment approved.",
                Data = newAchievements.ConvertAll(a => new AchievementDTO(a))
            });
        }

        [Authorize]
        [HttpDelete]
        [Route("delete/{assignmentId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int assignmentId)
        {
            Assignment? assignment = await this.assignmentRepository.GetAssignmentByID(assignmentId);

            if (assignment == null)
                throw new ApiException(404, "Assignment not found");

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

        #endregion

    }
}

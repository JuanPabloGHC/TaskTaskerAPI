using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/member")]
    public class MemberController : AppControllerBase
    {
        #region DATA MEMBERS

        private IMemberRepository memberRepository;

        #endregion

        #region CONSTRUCTOR

        public MemberController(IMemberRepository memberRepository)
        {
            this.memberRepository = memberRepository;
        }

        #endregion

        #region ENDPOINTS

        [Authorize]
        [HttpGet]
        [Route("get-all/{homeId:int}")]
        public async Task<IActionResult> GetAll([FromRoute] int homeId)
        {
            try
            {
                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), homeId);

                RequireMembership(caller);

                List<Member> members = (List<Member>)await this.memberRepository.GetHomeMembers(homeId);

                return Ok(new ApiResponse<List<MemberDTO>>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = members.ConvertAll(a => new MemberDTO(a, [], []))
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("get/{memberId:int}")]
        public async Task<IActionResult> GetByID([FromRoute] int memberId)
        {
            try
            {
                Member? member = await this.memberRepository.GetMemberByID(memberId);

                if (member == null)
                    throw new Exception("404;Member not found");

                // The caller may view a member if it is themselves, or if they belong
                // to the same home.
                if (member.person_id != this.GetPersonId())
                {
                    Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), member.home_id);

                    RequireMembership(caller);
                }

                IAttainmentRepository attainmentRepository;
                attainmentRepository = (IAttainmentRepository)HttpContext.RequestServices.GetService(typeof(IAttainmentRepository))!;

                IAssignmentRepository assignmentRepository;
                assignmentRepository = (IAssignmentRepository)HttpContext.RequestServices.GetService(typeof(IAssignmentRepository))!;

                if (attainmentRepository == null || assignmentRepository == null)
                    throw new Exception("500;Internal server error");

                List<Attainment> attainments = (List<Attainment>)await attainmentRepository.GetMemberAttainmentes(memberId);
                List<Assignment> assignments = (List<Assignment>)await assignmentRepository.GetMemberAssignments(memberId);

                MemberDTO memberDTO = new MemberDTO(member,
                    attainments.ConvertAll(a => new AchievementDTO(a.achievement)),
                    assignments.ConvertAll(a => new AssignmentDTO(a))
                );

                return Ok(new ApiResponse<MemberDTO>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = memberDTO
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
        public async Task<IActionResult> Create([FromBody] MemberDTO memberDTO)
        {
            try
            {
                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), memberDTO.home.id);

                RequireRole(caller, "Owner", "Admin");

                await this.memberRepository.CreateMember(memberDTO);

                await this.memberRepository.SaveChanges();

                return Created("", new ApiResponse<string>
                {
                    StatusCode = 201,
                    Message = "Member added successfully",
                    Data = String.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("update/{memberId:int}")]
        public async Task<IActionResult> Update([FromBody] MemberDTO memberDTO, [FromRoute] int memberId)
        {
            try
            {
                if (memberId != memberDTO.id)
                    throw new Exception("400;ID does not match");

                Member? target = await this.memberRepository.GetMemberByID(memberId);

                if (target == null)
                    throw new Exception("404;Member not found");

                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), target.home_id);

                RequireRole(caller, "Owner");

                await this.memberRepository.UpdateMember(memberDTO);

                await this.memberRepository.SaveChanges();

                return Ok(new ApiResponse<string>
                {
                    StatusCode = 200,
                    Message = "Member updated successfully",
                    Data = String.Empty
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("remove/{memberId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int memberId)
        {
            try
            {
                Member? target = await this.memberRepository.GetMemberByID(memberId);

                if (target == null)
                    throw new Exception("404;Member not found");

                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), target.home_id);

                RequireRole(caller, "Owner");

                await this.memberRepository.DeleteMember(memberId);

                await this.memberRepository.SaveChanges();

                return StatusCode(204, new ApiResponse<string>
                {
                    StatusCode = 204,
                    Message = "Member deleted successfully",
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

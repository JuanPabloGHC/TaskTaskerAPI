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
        private IAttainmentRepository attainmentRepository;
        private IAssignmentRepository assignmentRepository;

        #endregion

        #region CONSTRUCTOR

        public MemberController(
            IMemberRepository memberRepository,
            IAttainmentRepository attainmentRepository,
            IAssignmentRepository assignmentRepository)
        {
            this.memberRepository = memberRepository;
            this.attainmentRepository = attainmentRepository;
            this.assignmentRepository = assignmentRepository;
        }

        #endregion

        #region ENDPOINTS

        [Authorize]
        [HttpGet]
        [Route("get-all/{homeId:int}")]
        public async Task<IActionResult> GetAll([FromRoute] int homeId)
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

        [Authorize]
        [HttpGet]
        [Route("get/{memberId:int}")]
        public async Task<IActionResult> GetByID([FromRoute] int memberId)
        {
            Member? member = await this.memberRepository.GetMemberByID(memberId);

            if (member == null)
                throw new ApiException(404, "Member not found");

            // The caller may view a member if it is themselves, or if they belong
            // to the same home.
            if (member.person_id != this.GetPersonId())
            {
                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), member.home_id);

                RequireMembership(caller);
            }

            List<Attainment> attainments = (List<Attainment>)await this.attainmentRepository.GetMemberAttainmentes(memberId);
            List<Assignment> assignments = (List<Assignment>)await this.assignmentRepository.GetMemberAssignments(memberId);

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

        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] MemberDTO memberDTO)
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

        [Authorize]
        [HttpPost]
        [Route("add-by-phone")]
        public async Task<IActionResult> AddByPhone([FromBody] AddMemberByPhoneDTO request)
        {
            Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), request.homeId);

            RequireRole(caller, "Owner", "Admin");

            Member added = await this.memberRepository.CreateMemberByPhone(request.homeId, request.phone, request.roleId);

            await this.memberRepository.SaveChanges();

            return Created("", new ApiResponse<MemberDTO>
            {
                StatusCode = 201,
                Message = "Member added successfully",
                Data = new MemberDTO(added, [], [])
            });
        }

        [Authorize]
        [HttpPatch]
        [Route("update/{memberId:int}")]
        public async Task<IActionResult> Update([FromBody] MemberDTO memberDTO, [FromRoute] int memberId)
        {
            if (memberId != memberDTO.id)
                throw new ApiException(400, "ID does not match");

            Member? target = await this.memberRepository.GetMemberByID(memberId);

            if (target == null)
                throw new ApiException(404, "Member not found");

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

        [Authorize]
        [HttpDelete]
        [Route("remove/{memberId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int memberId)
        {
            Member? target = await this.memberRepository.GetMemberByID(memberId);

            if (target == null)
                throw new ApiException(404, "Member not found");

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

        #endregion

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/attainment")]
    public class AttainmentController : AppControllerBase
    {
        #region DATA MEMBERS

        private IAttainmentRepository attainmentRepository;
        private IMemberRepository memberRepository;

        #endregion

        #region CONSTRUCTOR

        public AttainmentController(IAttainmentRepository attainmentRepository, IMemberRepository memberRepository)
        {
            this.attainmentRepository = attainmentRepository;
            this.memberRepository = memberRepository;
        }

        #endregion

        #region ENDPOINTS

        [Authorize]
        [HttpGet]
        [Route("member/{memberId:int}")]
        public async Task<IActionResult> GetMemberAttainments([FromRoute] int memberId)
        {
            Member? target = await this.memberRepository.GetMemberByID(memberId);

            if (target == null)
                throw new ApiException(404, "Member not found");

            // A member sees their own; Owners/Admins can see any member's.
            if (target.person_id != this.GetPersonId())
            {
                Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), target.home_id);

                RequireRole(caller, "Owner", "Admin");
            }

            List<Attainment> attainments = (List<Attainment>)await this.attainmentRepository.GetMemberAttainmentes(memberId);

            return Ok(new ApiResponse<List<AchievementDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = attainments.ConvertAll(a => new AchievementDTO(a.achievement))
            });
        }

        [Authorize]
        [HttpGet]
        [Route("home/{homeId:int}")]
        public async Task<IActionResult> GetHomeAttainments([FromRoute] int homeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), homeId);

            RequireMembership(caller);

            (page, pageSize) = NormalizePaging(page, pageSize);

            (IEnumerable<Attainment> items, int total) = await this.attainmentRepository.GetHomeAttainmentesPaged(homeId, page, pageSize);

            return Ok(new ApiResponse<PagedResult<AttainmentDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = new PagedResult<AttainmentDTO>(items.Select(a => new AttainmentDTO(a)).ToList(), page, pageSize, total)
            });
        }

        #endregion

    }
}

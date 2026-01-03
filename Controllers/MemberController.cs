using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;
using static TaskTaskerAPI.Controllers.HomeController;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/member")]
    public class MemberController : Controller
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

        [HttpGet]
        [Route("get-all/{homeId:int}/{memberId:int}")]
        public async Task<IActionResult> GetAll([FromRoute] int homeId, [FromRoute] int memberId)
        {
            try
            {
                if (!await this.memberRepository.IsMemberOfHome(memberId, homeId))
                    throw new Exception("403;You do not have permission to view the members of this home");

                List<Member> members = (List<Member>)await this.memberRepository.GetHomeMembers(homeId);

                List<MemberDTO> memberDTOs = new List<MemberDTO>();

                foreach (Member member in members)
                {
                    memberDTOs.Add(new MemberDTO(member, [], []));
                }

                return Ok(new ApiResponse<List<MemberDTO>>
                {
                    StatusCode = 200,
                    Message = "",
                    Data = memberDTOs
                });
            }
            catch (Exception ex)
            {
                return this.CatchReturn(ex);
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] MemberDTO memberDTO)
        {
            try
            {
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

        [HttpPatch]
        [Route("update/{memberId:int}")]
        public async Task<IActionResult> Update([FromBody] MemberDTO memberDTO, [FromRoute] int memberId)
        {
            try
            {
                if (memberId != memberDTO.id)
                    throw new Exception("400;ID does not match");

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

        [HttpDelete]
        [Route("remove/{memberId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int memberId)
        {
            try
            {
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

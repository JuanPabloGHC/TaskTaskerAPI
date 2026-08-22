using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.DTOs;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.DAL.Interfaces;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    [ApiController]
    [Route("api/home")]
    public class HomeController : AppControllerBase
    {
        #region DATA MEMBERS

        private IHomeRepository homeRepository;
        private IMemberRepository memberRepository;

        #endregion

        #region CONSTRUCTOR

        public HomeController(IHomeRepository homeRepository, IMemberRepository memberRepository)
        {
            this.homeRepository = homeRepository;
            this.memberRepository = memberRepository;
        }

        #endregion

        #region ENDPOINTS

        [Authorize]
        [HttpGet]
        [Route("mine")]
        public async Task<IActionResult> GetMine()
        {
            List<Member> memberships = (List<Member>)await this.memberRepository.GetMemberHomes(this.GetPersonId());

            return Ok(new ApiResponse<List<MemberDTO>>
            {
                StatusCode = 200,
                Message = "",
                Data = memberships.ConvertAll(m => new MemberDTO(m, [], []))
            });
        }

        [Authorize]
        [HttpGet]
        [Route("get/{id:int}")]
        public async Task<IActionResult> GetByID([FromRoute] int id)
        {
            Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), id);

            RequireMembership(caller);

            Home? home = await this.homeRepository.GetHomeByID(id);

            if (home == null)
                throw new ApiException(404, "Home not found");

            return Ok(new ApiResponse<HomeDTO>
            {
                StatusCode = 200,
                Message = "",
                Data = new HomeDTO(home)
            });
        }

        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] HomeDTO homeDTO)
        {
            Home home = await this.homeRepository.CreateHome(homeDTO, this.GetPersonId());

            await this.homeRepository.SaveChanges();

            return Created("", new ApiResponse<HomeDTO>
            {
                StatusCode = 201,
                Message = "Home created successfully",
                Data = new HomeDTO(home)
            });
        }

        [Authorize]
        [HttpPatch]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] HomeDTO homeDTO, [FromRoute] int id)
        {
            if (id != homeDTO.id)
                throw new ApiException(400, "ID does not match");

            Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), id);

            RequireRole(caller, "Owner", "Admin");

            Home home = await this.homeRepository.UpdateHome(homeDTO);

            await this.homeRepository.SaveChanges();

            return Ok(new ApiResponse<HomeDTO>
            {
                StatusCode = 200,
                Message = "Home modified successfully",
                Data = new HomeDTO(home)
            });
        }

        [Authorize]
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            Member? caller = await this.memberRepository.GetMemberByPersonAndHome(this.GetPersonId(), id);

            RequireRole(caller, "Owner");

            await this.homeRepository.DeleteHome(id);

            await this.homeRepository.SaveChanges();

            return StatusCode(204, new ApiResponse<string>
            {
                StatusCode = 204,
                Message = "Home deleted successfully",
                Data = String.Empty
            });
        }

        #endregion

    }
}

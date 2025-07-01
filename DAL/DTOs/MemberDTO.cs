using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class MemberDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public PersonDTO person { get; set; }

        public HomeDTO home { get; set; }

        public RoleDTO role { get; set; }

        public List<AchievementDTO> achievements { get; set; }

        public List<AssignmentDTO> assignments { get; set; }

        #endregion

        #region CONSTRUCTORS

        public MemberDTO() { }

        public MemberDTO(int id, PersonDTO person, HomeDTO home, RoleDTO role, List<AchievementDTO> achievements, List<AssignmentDTO> assignments)
        {
            this.id = id;
            this.person = person;
            this.home = home;
            this.role = role;
            this.achievements = achievements;
            this.assignments = assignments;
        }

        public MemberDTO(Member member, List<AchievementDTO> achievements, List<AssignmentDTO> assignments)
        {
            this.id = member.id;
            this.person = new PersonDTO(member.person);
            this.home = new HomeDTO(member.home);
            this.role = new RoleDTO(member.role);
            this.achievements = achievements;
            this.assignments = assignments;
        }

        #endregion

    }
}

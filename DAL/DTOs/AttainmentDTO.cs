using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class AttainmentDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public int member_id { get; set; }

        public PersonDTO? person { get; set; }

        public AchievementDTO achievement { get; set; } = new AchievementDTO();

        #endregion

        #region CONSTRUCTORS

        public AttainmentDTO() { }

        public AttainmentDTO(Attainment attainment)
        {
            this.id = attainment.id;
            this.member_id = attainment.member_id;
            this.person = attainment.member != null ? new PersonDTO(attainment.member.person) : null;
            this.achievement = new AchievementDTO(attainment.achievement);
        }

        #endregion

    }
}

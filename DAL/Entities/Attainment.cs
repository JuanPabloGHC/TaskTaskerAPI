using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Attainment")]
    public class Attainment
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        #endregion

        #region NAVIGATION

        [Required]
        public int member_id { get; set; }
        [ForeignKey("member_id")]
        public Member member { get; set; } = null!;

        [Required]
        public int achievement_id { get; set; }
        [ForeignKey("achievement_id")]
        public Achievement achievement { get; set; } = null!;

        #endregion

        #region CONSTRUCTORS

        public Attainment() { }

        public Attainment(int id, int memberID, int achievementID)
        {
            this.id = id;
            this.member_id = memberID;
            this.achievement_id = achievementID;
        }

        #endregion

    }
}

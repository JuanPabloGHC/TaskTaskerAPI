using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskTaskerAPI.DAL.DTOs;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Achievement")]
    public class Achievement
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        public string name { get; set; }

        [Required]
        public string description { get; set; }

        [Required]
        public string image { get; set; }


        #endregion

        #region CONSTRUCTORS

        public Achievement() { }

        public Achievement(AchievementDTO achievementDTO)
        {
            this.id = achievementDTO.id;
            this.name = achievementDTO.name;
            this.description = achievementDTO.description;
            this.image = achievementDTO.image;
        }

        #endregion

    }
}

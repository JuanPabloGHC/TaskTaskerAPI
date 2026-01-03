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
        public int days { get; set; }

        [Required]
        public string image { get; set; }

        #endregion

        #region NAVIGATION

        [Required]
        public int task_id { get; set; }
        [ForeignKey("task_id")]
        public Task task { get; set; }

        #endregion

        #region CONSTRUCTORS

        public Achievement() { }

        public Achievement(AchievementDTO achievementDTO)
        {
            this.id = achievementDTO.id;
            this.name = achievementDTO.name;
            this.days = achievementDTO.days;
            this.image = achievementDTO.image;
            this.task_id = achievementDTO.task.id;
        }

        #endregion

    }
}

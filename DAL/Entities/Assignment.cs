using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskTaskerAPI.DAL.DTOs;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Assignment")]
    public class Assignment
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        [Required]
        public DateTime date { get; set; }

        #endregion

        #region NAVIGATION

        [Required]
        public int member_id { get; set; }
        [ForeignKey("member_id")]
        public Member member { get; set; }

        [Required]
        public int status_id { get; set; }
        [ForeignKey("status_id")]
        public Status status { get; set; }

        [Required]
        public int task_id { get; set; }
        [ForeignKey("task_id")]
        public Task task { get; set; }

        #endregion

        #region CONSTRUCTORS

        public Assignment() { }

        public Assignment(AssignmentDTO assignmentDTO)
        {
            this.id = assignmentDTO.id;
            this.date = assignmentDTO.date;
            this.member_id = assignmentDTO.member.id;
            this.status_id = assignmentDTO.status.id;
            this.task_id = assignmentDTO.task.id;
        }

        #endregion

    }
}

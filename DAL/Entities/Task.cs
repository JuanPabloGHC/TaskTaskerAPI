using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskTaskerAPI.DAL.DTOs;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Task")]
    public class Task
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(25)]
        public string name { get; set; }

        [Required]
        public string image { get; set; }

        #endregion

        #region CONSTRUCTORS

        public Task() { }

        public Task(TaskDTO taskDTO)
        {
            this.id = taskDTO.id;
            this.name = taskDTO.name;
            this.image = taskDTO.image;
        }

        #endregion

    }
}

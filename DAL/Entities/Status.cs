using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskTaskerAPI.DAL.DTOs;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Status")]
    public class Status
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(25)]
        public string name { get; set; } = string.Empty;

        [Required]
        [StringLength(7)]
        public string color { get; set; } = string.Empty;

        #endregion

        #region CONSTRUCTORS

        public Status() { }

        public Status(StatusDTO statusDTO)
        {
            this.id = statusDTO.id;
            this.name = statusDTO.name;
            this.color = statusDTO.color;
        }

        #endregion

    }
}

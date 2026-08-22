using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskTaskerAPI.DAL.DTOs;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Home")]
    public class Home
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(25)]
        public string name { get; set; } = string.Empty;

        [Required]
        public string image { get; set; } = string.Empty;

        #endregion

        #region CONSTRUCTORS

        public Home() { }

        public Home(HomeDTO homeDTO)
        {
            this.id = homeDTO.id;
            this.name = homeDTO.name;
            this.image = homeDTO.image;
        }

        #endregion

    }
}

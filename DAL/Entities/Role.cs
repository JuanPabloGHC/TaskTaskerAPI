using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskTaskerAPI.DAL.DTOs;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Role")]
    public class Role
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

        public Role() { }

        public Role(RoleDTO roleDTO)
        {
            this.id = roleDTO.id;
            this.name = roleDTO.name;
            this.image = roleDTO.image;
        }

        #endregion

    }
}

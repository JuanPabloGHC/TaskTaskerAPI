using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("AdminUser")]
    public class AdminUser
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        public string username { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string password { get; set; } = string.Empty;

        #endregion

        #region CONSTRUCTORS

        public AdminUser() { }

        #endregion

    }
}

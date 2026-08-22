using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TaskTaskerAPI.DAL.DTOs;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Person")]
    public class Person
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(10)]
        public string phone { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        public string name { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string password { get; set; } = string.Empty;

        [Required]
        public string image { get; set; } = string.Empty;

        #endregion

        #region CONSTRUCTORS

        public Person() { }

        public Person(PersonDTO personDTO)
        {
            this.id = personDTO.id;
            this.phone = personDTO.phone;
            this.name = personDTO.name;
            this.password = personDTO.password;
            this.image = personDTO.image;
        }

        #endregion

    }
}

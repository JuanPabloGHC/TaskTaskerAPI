using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class PersonDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public string phone { get; set; }

        public string name { get; set; }

        public string password { get; set; }

        public string image { get; set; }

        #endregion

        #region CONSTRUCTORS

        public PersonDTO() { }

        public PersonDTO(int id, string phone, string name, string password, string image)
        {
            this.id = id;
            this.phone = phone;
            this.name = name;
            this.password = password;
            this.image = image;
        }

        public PersonDTO(Person person)
        {
            this.id = person.id;
            this.phone = person.phone;
            this.name = person.name;
            this.image = person.image;
        }

        #endregion

    }
}

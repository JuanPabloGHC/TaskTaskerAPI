using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class StatusDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public string name { get; set; } = string.Empty;

        public string color { get; set; } = string.Empty;

        #endregion

        #region CONSTRUCTORS

        public StatusDTO() { }

        public StatusDTO(int id, string name, string color)
        {
            this.id = id;
            this.name = name;
            this.color = color;
        }

        public StatusDTO(Status status)
        {
            this.id = status.id;
            this.name = status.name;
            this.color = status.color;
        }

        #endregion

    }
}

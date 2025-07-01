using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class TaskDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public string name { get; set; }

        public string image { get; set; }

        #endregion

        #region CONSTRUCTORS

        public TaskDTO() { }

        public TaskDTO(int id, string name, string image)
        {
            this.id = id;
            this.name = name;
            this.image = image;
        }

        public TaskDTO(Entities.Task task)
        {
            this.id = task.id;
            this.name = task.name;
            this.image = task.image;
        }

        #endregion

    }
}

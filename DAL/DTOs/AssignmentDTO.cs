namespace TaskTaskerAPI.DAL.DTOs
{
    public class AssignmentDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public MemberDTO member { get; set; }

        public DateTime date { get; set; }

        public StatusDTO status { get; set; }

        public TaskDTO task { get; set; }

        #endregion

        #region CONSTRUCTORS

        public AssignmentDTO() { }

        public AssignmentDTO(int id, MemberDTO member, DateTime date, StatusDTO status, TaskDTO task)
        {
            this.id = id;
            this.member = member;
            this.date = date;
            this.status = status;
            this.task = task;
        }

        #endregion
    }
}

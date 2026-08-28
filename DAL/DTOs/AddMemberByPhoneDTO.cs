namespace TaskTaskerAPI.DAL.DTOs
{
    public class AddMemberByPhoneDTO
    {
        public int homeId { get; set; }

        public string phone { get; set; } = string.Empty;

        public int roleId { get; set; }
    }
}

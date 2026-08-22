namespace TaskTaskerAPI.DAL.DTOs
{
    public class AuthResponseDTO
    {
        public string accessToken { get; set; } = string.Empty;

        public string refreshToken { get; set; } = string.Empty;

        public PersonDTO person { get; set; } = new PersonDTO();
    }
}

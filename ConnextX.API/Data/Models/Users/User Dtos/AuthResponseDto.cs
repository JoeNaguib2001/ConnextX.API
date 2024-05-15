namespace ConnextX.API.Data.Models.Users
{
    public class AuthResponseDto
    {
        public string FirstName { get; set; }   

        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string UserName { get; set; }

        public string Token { get; set; }
    }
}

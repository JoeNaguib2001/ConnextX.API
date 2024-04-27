namespace ConnextX.API.Data.Models.Users
{
    public class ApiUserDto : LoginDto
    {
        //This is For Registeration , It Inherites From LoginDto The UserName and Password Properties
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}

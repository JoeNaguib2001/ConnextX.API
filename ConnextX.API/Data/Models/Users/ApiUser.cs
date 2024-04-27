using Microsoft.AspNetCore.Identity;

namespace ConnextX.API.Data.Models.Users
{
    public class ApiUser : IdentityUser
    {
        //Regular User + I Added FirstName and LastName to Its Properties
        //We Used This Class For Seeding The Database
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        ICollection<Subscription> Subscriptions { get; set; }
    }

}

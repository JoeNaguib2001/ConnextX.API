using ConnextX.API.Data.Models.Users;

namespace ConnextX.API.Data.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Quota { get; set; }

        
        public string UserId { get; set; }
        public ApiUser User { get; set; }

        public int PlanId { get; set; }
        public Plan Plan { get; set; }
        
    }

}

namespace ConnextX.API.Data.Dtos
{
    public class subscriptionDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string userId { get; set; }

        public int Quota { get; set; }

        public int PlanId { get; set; }

    }
}

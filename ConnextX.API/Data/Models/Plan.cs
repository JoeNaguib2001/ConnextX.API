namespace ConnextX.API.Data.Models
{
    public class Plan
    {
        public int PlanId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quota { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; } 

    }
}

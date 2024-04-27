namespace ConnextX.API.Data.Models
{
    public class Language
    {
        public int Id { get; set; }

        [MaxLength(length: 20)]
        public string Name { get; set; }
    }
}

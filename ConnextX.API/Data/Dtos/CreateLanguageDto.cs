namespace ConnextX.API.Data.Dtos
{
    public class CreateLanguageDto
    {
        [MaxLength(length: 20)]
        public string Name { get; set; }
    }
}

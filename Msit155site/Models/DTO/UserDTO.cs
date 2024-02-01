namespace Msit155site.Models.DTO
{
    public class UserDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int Age { get; set; }
        public IFormFile? Photo {  get; set; }
    }
}

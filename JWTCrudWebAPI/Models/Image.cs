using System.Text.Json.Serialization;

namespace JWTCrudWebAPI.Models
{
    public class Image
    {
        public int ImageId { get; set; }
        public byte[] Base64Image { get; set; }
        public Guid Id { get; set; }
        public Employee Employee { get; set; } // Navigation property

    }
}

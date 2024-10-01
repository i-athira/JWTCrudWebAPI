namespace JWTCrudWebAPI.Models
{
    public class User
    {
        public int Id { get; set; }=0;
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int IsActive { get; set; } = 1;
        public DateTime CreateOn { get; set; }= DateTime.Now;



        // Add these for refresh token functionality
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }


    }
}

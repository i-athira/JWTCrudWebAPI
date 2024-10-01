using JWTCrudWebAPI.Data;
using JWTCrudWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWTCrudWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;
        public UsersController(ApplicationDbContext dbContext,IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration(UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }
            var objUser = dbContext.Users.FirstOrDefault(x => x.Email == userDto.Email);
            if (objUser == null)
            {
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Set refresh token expiry to 7 days



                dbContext.Users.Add(new User
                {
                    Firstname = userDto.Firstname,
                    Lastname = userDto.Lastname,
                    Email = userDto.Email,
                    Password = userDto.Password,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = refreshTokenExpiryTime
                });
                dbContext.SaveChanges();
                return Ok("User registered successfully");
            }
            else
            {
                return BadRequest("User already exist with the same Email Address");
            }
        }





        //[HttpPost]
        //[Route("Login")]
        //public IActionResult Login(LoginDto loginDto)
        //{
        //    var user = dbContext.Users.FirstOrDefault(x => x.Email == loginDto.Email && x.Password == loginDto.Password);
        //    if (user != null)
        //    {
        //        var claims = new[]
        //        {
        //            new Claim(JwtRegisteredClaimNames.Sub,configuration["Jwt:Subject"]),
        //            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
        //            new Claim("UserId",user.Id.ToString()),
        //            new Claim("Email",user.Email.ToString())
        //        };
        //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        //        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //        var token = new JwtSecurityToken(
        //            configuration["Jwt:Issuer"],
        //            configuration["Jwt:Audience"],
        //            claims,
        //            expires: DateTime.UtcNow.AddMinutes(10),
        //            signingCredentials: signIn
        //            );
        //        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        //        return Ok(new { Token = tokenValue, User = user });
        //    }
        //    return NoContent();
        //}






        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginDto loginDto)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Email == loginDto.Email && x.Password == loginDto.Password);
            if (user != null)
            {

                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();


                // Store refresh token and expiry time
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                dbContext.SaveChanges();

                return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });

            }

            return NoContent();
        }



        [HttpPost]
        [Route("RefreshToken")]
        public IActionResult RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            var principal = GetPrincipalFromExpiredToken(tokenRequest.AccessToken);
            var email = principal.FindFirstValue("Email");

            var user = dbContext.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || user.RefreshToken != tokenRequest.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized("Invalid refresh token or token expired");
            }

            // Generate new tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            dbContext.SaveChanges();

            return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
        }


        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Email", user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                configuration["Jwt:Issuer"],
                configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signIn);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }


        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false // ignore expiration
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }









        [HttpGet]
        [Route("GetUsers")]
        public IActionResult GetUsers()
        {
            return Ok(dbContext.Users.ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        public IActionResult GetUser(int id)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
          
                return Ok(user);
            else
                return NoContent();

        }
    }
}

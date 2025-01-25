using Azure.Core;
using JWTCrudWebAPI.Data;
using JWTCrudWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
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
        private readonly ILogger<UsersController> logger;

        public UsersController(ApplicationDbContext dbContext,IConfiguration configuration, ILogger<UsersController> logger)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Registration(UserDto userDto)
        {
            logger.LogInformation("Registration attempt for email: {Email}", userDto.Email);

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Registration failed due to invalid model state.");

                return BadRequest(ModelState);

            }
            var objUser =await dbContext.Users.FirstOrDefaultAsync(x => x.Email == userDto.Email);
            if (objUser == null)
            {
                try
                {
                    var refreshToken = GenerateRefreshToken();
                    var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Set refresh token expiry to 7 days



                    await dbContext.Users.AddAsync(new User
                    {
                        Firstname = userDto.Firstname,
                        Lastname = userDto.Lastname,
                        Email = userDto.Email,
                        Password = userDto.Password,
                        RefreshToken = refreshToken,
                        RefreshTokenExpiryTime = refreshTokenExpiryTime
                    });
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("User registered successfully: {Email}", userDto.Email);

                    return Ok("User registered successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred during user registration for email: {Email}", userDto.Email);
                    return StatusCode(500, "An error occurred while registering the user.");

                }
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
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email && x.Password == loginDto.Password);
            if (user != null)
            {
                try
                {
                    var accessToken = GenerateAccessToken(user);
                    var refreshToken = GenerateRefreshToken();


                    // Store refresh token and expiry time
                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Login successful for email: {Email}", loginDto.Email);

                    return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred during login for email: {Email}", loginDto.Email);
                    return StatusCode(500, "An error occurred while processing the login request.");
                }
            }

            return NoContent();
        }



        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            logger.LogInformation("Refresh token attempt for access token.");
            try
            {
                var principal = GetPrincipalFromExpiredToken(tokenRequest.AccessToken);
                var email = principal.FindFirstValue("Email");

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null || user.RefreshToken != tokenRequest.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    logger.LogWarning("Invalid refresh token or token expired for email: {Email}", email);

                    return Unauthorized("Invalid refresh token or token expired");
                }

                // Generate new tokens
                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Refresh token successful for email: {Email}", email);

                return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during token refresh.");
                return StatusCode(500, "An error occurred while refreshing the token.");
            }
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
        public async Task<IActionResult> GetUsers()
        {
            logger.LogInformation("Fetching all users.");

            var users = await dbContext.Users.ToListAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            logger.LogInformation("Fetching user with ID: {Id}", id);

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)

            {
                logger.LogInformation("User found with ID: {Id}", id);

                return Ok(user);
            }
               
            else
            {
                logger.LogWarning("No user found with ID: {Id}", id);
                return NoContent();
            }
        }
    }
}
//Used logger.LogInformation() to log successful operations.
//Used logger.LogWarning() for warnings(e.g., invalid requests).
//Used logger.LogError() to log exceptions.
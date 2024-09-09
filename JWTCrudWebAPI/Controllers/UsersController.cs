using JWTCrudWebAPI.Data;
using JWTCrudWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                dbContext.Users.Add(new User
                {
                    Firstname = userDto.Firstname,
                    Lastname = userDto.Lastname,
                    Email = userDto.Email,
                    Password = userDto.Password
                });
                dbContext.SaveChanges();
                return Ok("User registered successfully");
            }
            else
            {
                return BadRequest("User already exist with the same Email Address");
            }
        }


        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginDto loginDto)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Email == loginDto.Email && x.Password == loginDto.Password);
            if (user != null)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub,configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim("UserId",user.Id.ToString()),
                    new Claim("Email",user.Email.ToString())
                };
                var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                var signIn=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
                var token=new JwtSecurityToken(
                    configuration["Jwt:Issuer"],
                    configuration["Jwt:Audience"],
                    claims,
                    expires:DateTime.UtcNow.AddMinutes(60),
                    signingCredentials:signIn
                    );
                string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new {Token =tokenValue,User=user});
            }
            return NoContent();
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

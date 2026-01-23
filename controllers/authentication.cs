using Microsoft.AspNetCore.Mvc;
using blog_app_ai_dotnet.models;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        DefaultContext _context;
        IConfiguration _configuration;
        public LoginController(DefaultContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult LoginPost(UserDTO user_request)
        {
            AuthenticationUser? user = _context.AuthenticationUsers.SingleOrDefault(u=>user_request.Username==u.Username);
            if (user == null)
            {
                return Unauthorized();
            }
            string token = AuthMethods.CreateJwtToken(
                id: (int)user.Id,
                username: user.Username,
                context: _context,
                configuration: _configuration
            );
            return Ok(token);
        }
    }

    public class UserDTO
    {
        public required string Username {get; set;}
        public required string Password {get; set;}
    }

    static public class AuthMethods
    {
        static public string CreateJwtToken(int id, string username, DefaultContext context, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                //audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(jwtSettings["DurationInMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
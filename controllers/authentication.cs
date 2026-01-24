using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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
        public async Task<IActionResult> LoginPost(UserDTO user_request)
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri((_configuration["BackendOrigin"]+"/auth/checkuser/")??"")
            };
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(_configuration["Admin:Username"]+":"+_configuration["Admin:Password"])
                ));
            var values = new Dictionary<string, string>
                {
                    { "username", user_request.Username },
                    { "password", user_request.Password }
                };
            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("", content);
            if (!response.IsSuccessStatusCode)return Unauthorized();
            var data = await response.Content.ReadFromJsonAsync<ResponseDTO>();
            if(data is null || !data.State)return Unauthorized();
            AuthenticationUser? user = _context.AuthenticationUsers.SingleOrDefault(u=>
                data.Username==u.Username && data.Password==u.Password
            );
            if (user == null)return Unauthorized();
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

    public class ResponseDTO
    {
        public required bool State {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}
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
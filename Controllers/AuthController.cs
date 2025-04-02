using BlogProject.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username == "admin" && request.Password == "blog58/*")
            {
                var token = _jwtService.GenerateToken(request.Username);
                return Ok(new { token });
            }

            return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre" });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}

using Microsoft.AspNetCore.Mvc;
using StoreManagementAPI.Middlewares;
using StoreManagementAPI.Models;
using StoreManagementAPI.Models.RequestSchemas;
using StoreManagementAPI.Services;

namespace StoreManagementAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly UserService _userService;
        private readonly JWTTokenService _tokenService;
        private readonly ResetPasswordTokenService _rPTService;

        public AuthController(UserService userService, JWTTokenService tokenService, ResetPasswordTokenService rPTService)
        {
            _rPTService = rPTService;
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest body)
        {
            if (string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
                return BadRequest(new { message = "Please fill in all fields!" });


            string username = body.Username;
            string password = body.Password;

            var user = await _userService.GetByUsername(username);
            if (user == null || !PasswordService.VerifyPassword(password, user.Password))
                return BadRequest(new { message = "Invalid credentials!" });

            if (user.Status.ToString().Equals("LOCKED"))
                return BadRequest(new { message = "Please contact admin to unlock your account!" });

            if (user.Username.Equals(password))
                return BadRequest(new { message = "Please contact admin to reset your password!" });

            string token = _tokenService.GenerateToken(user);
            Response.Headers["Authorization"] = "Bearer " + token;

            return Ok(new
            {
                message = "Login success",
                user = user,
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery(Name = "token")] string token, [FromBody] Dictionary<string, string> body)
        {
            string newPassword = body["newPassword"];
            string confirmPassword = body["confirmPassword"];

            if (string.IsNullOrEmpty(token))
                return NotFound(new { message = "Invalid token" });

            string? userId = await _rPTService.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
                return NotFound(new { message = "Invalid token" });

            User user = await _userService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "Invalid token" });

            if (string.IsNullOrEmpty(newPassword))
                return BadRequest(new { message = "Password not found" });

            if (newPassword != confirmPassword)
                return BadRequest(new { message = "Password and confirm password do not match" });

            if (newPassword == user.Username)
                return BadRequest(new { message = "Password must be different from the username" });

            string hashedPassword = PasswordService.HashPassword(newPassword);
            user.Password = hashedPassword;

            bool isUpdated = await _userService.UpdateUser(user.Id, user);
            if (!isUpdated)
                return BadRequest(new { message = "Reset password failed" });

            return Ok(new { message = "Reset password successfully. Please login again!", user });
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromHeader(Name = "Authorization")] string token, [FromBody] Dictionary<string, string> body)
        {
            string resource = body["resource"];

            token = token.Substring(7);
            string? email = _tokenService.ValidateToken(token);
            if (email == null)
                return BadRequest(new { message = "Invalid token" });

            User user = await _userService.GetByEmail(email);

            if (user == null)
                return BadRequest(new { message = "Invalid token" });

            if (resource.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                if (user.Role != Role.ADMIN && user.Role != Role.OWNER)
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "No permission" });
            }
            else if (resource.Equals("OWNER", StringComparison.OrdinalIgnoreCase))
            {
                if (user.Role != Role.OWNER)
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "No permission" });
            }

            return Ok(new { message = "Valid token", user });
        }
    }
}

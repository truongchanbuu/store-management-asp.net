using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementAPI.Middlewares;
using StoreManagementAPI.Models;
using StoreManagementAPI.Models.RequestSchemas;
using StoreManagementAPI.Services;
using System.Net;
using System.Text.RegularExpressions;

namespace StoreManagementAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly MailService _mailService;
        private readonly ResetPasswordTokenService _rPTService;

        public UserController(UserService userService, MailService mailService, ResetPasswordTokenService rPTService)
        {
            _rPTService = rPTService;
            _userService = userService;
            _mailService = mailService;
        }

        [HttpGet]
        [Route("admin/users")]
        public async Task<IActionResult> GetUsers([FromQuery(Name = "text")] string searchText = "", [FromQuery(Name = "email")] string email = "")
        {
            IEnumerable<User> users;

            if (!string.IsNullOrEmpty(searchText))
                users = await _userService.SearchUser(searchText);
            else if(!string.IsNullOrEmpty(email))
                users = new List<User>() { await _userService.GetByEmail(email) };
            else
                users = await _userService.GetAllUsers();

            foreach (var user in users)
                user.Password = "";

            return Ok(new { message = "Get users success", users });
        }

        [HttpGet]
        [Route("admin/users/{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            var user = await _userService.GetById(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.Password = "";
            return Ok(new { message = "Get user success", user = user });
        }

        [HttpPost]
        [Route("admin/users/create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest body)
        {
            try
            {
                string email = body.Email;

                if (string.IsNullOrEmpty(email))
                    return BadRequest(new { message = "Please fill all fields" });

                string domain = _mailService.HOST.Split("smtp.")[1];
                if (!Regex.IsMatch(email, $"^\\w+@({domain})$"))
                    return BadRequest(new { message = "Invalid email or domain" });

                var existingUser = await _userService.GetByEmail(email);
                if (existingUser != null)
                    return BadRequest(new { message = "Email already exists" });

                string username = email.Split('@')[0];
                string password = PasswordService.HashPassword(username);

                User user = new User()
                {
                    Email = email,
                    Username = username,
                    Password = password,
                    Role = Role.EMPLOYEE,
                    Status = Status.NORMAL,
                };

                bool isAdded = await _userService.AddUser(user);

                if (!isAdded)
                    return BadRequest(new { message = "Create user failed" });

                var response = await ResetPassword(user.Id);
                if (response.StatusCode >= 400)
                    return response;

                return Ok(new
                {
                    message = "Create user success. A reset password mail has been sent to the user",
                    user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpPost("admin/users/update/{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserRequest body)
        {
            try
            {
                string role = body.Role.ToUpper();
                string status = body.Status.ToUpper();

                if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(status))
                    return BadRequest(new { message = "Please fill all fields" });

                if (!_userService.IsValidRole(role))
                    return BadRequest(new { message = "Invalid role" });

                if (!_userService.IsValidStatus(status))
                    return BadRequest(new { message = "Invalid status" });

                User user = await _userService.GetById(id);
                if (user == null)
                    return BadRequest(new { message = "User not found" });

                if(user.Role == Role.OWNER && status.Equals(Status.LOCKED))
                    return BadRequest(new { message = "Cannot lock owner" });

                user.Role = Enum.Parse<Role>(role);
                user.Status = Enum.Parse<Status>(status);

                bool isUpdated = await _userService.UpdateUser(id, user);
                if (!isUpdated)
                    return BadRequest(new { message = "Update user failed" });

                return Ok(new { message = "Update user success", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpPost("admin/users/delete/{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            try
            {
                User user = await _userService.GetById(id);
                if (user == null)
                    return BadRequest(new { message = "User not found" });

                if (user.Role == Role.OWNER)
                    return BadRequest(new { message = "Cannot delete owner" });

                bool isDeleted = await _userService.RemoveUser(id);
                if (!isDeleted)
                    return BadRequest(new { message = "Delete user failed" });

                return Ok(new { message = "Delete user success", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("admin/users/reset-password/{id}")]
        public async Task<ObjectResult> ResetPassword([FromRoute] string id)
        {
            try
            {
                User user = await _userService.GetById(id);
                if (user == null)
                    return BadRequest(new { message = "User not found" });

                string token = await _rPTService.CreateToken(id);
                if (token == null)
                    return BadRequest(new { message = "Create token failed" });

                if (!await _mailService.SendResetPasswordMail(user.Email, token))
                    return BadRequest(new { message = "Send reset password mail failed" });

                return Ok(new { message = "Send reset password mail success", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpPost("users/change-avatar/{id}")]
        public async Task<IActionResult> ChangeAvatar([FromRoute] string id, [FromBody] ChangeAvatarRequest body)
        {
            try
            {
                string avatarUrl = body.AvatarUrl;
                User user = await _userService.GetById(id);
                if (user == null)
                    return BadRequest(new { message = "User not found" });

                if (!Regex.IsMatch(avatarUrl, @"^https?://.*\.(?:png|jpg|jpeg|gif)$"))
                    return BadRequest(new { message = "Invalid avatar url" });

                user.Avatar = avatarUrl;
                bool isUpdated = await _userService.UpdateUser(id, user);
                if (!isUpdated)
                    return BadRequest(new { message = "Change avatar failed" });

                return Ok(new { message = "Change avatar success", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpPost("users/change-password/{id}")]
        public async Task<IActionResult> ChangePassword([FromRoute] string id, [FromBody] ResetPasswordRequest body)
        {
            try
            {
                string newPassword = body.NewPassword;
                string confirmPassword = body.ConfirmPassword;

                User user = await _userService.GetById(id);
                if (user == null)
                    return BadRequest(new { message = "User not found" });

                if (string.IsNullOrEmpty(newPassword))
                    return BadRequest(new { message = "Password is required" });

                if (newPassword != confirmPassword)
                    return BadRequest(new { message = "Confirm password not match" });

                if (newPassword == user.Username)
                    return BadRequest(new { message = "Password must be different from username" });

                newPassword = PasswordService.HashPassword(newPassword);
                user.Password = newPassword;
                bool isUpdated = await _userService.UpdateUser(id, user);
                if (!isUpdated)
                    return BadRequest(new { message = "Change password failed" });

                return Ok(new { message = "Change password success", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet("users/total")]
        public IActionResult GetTotalUser()
        {
            return Ok(new
            {
                code = HttpStatusCode.OK,
                message = "Success",
                data = new List<long> { _userService.GetTotalUser() }
            });
        }

    }
}

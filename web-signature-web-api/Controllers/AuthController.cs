using Microsoft.AspNetCore.Mvc;
using web_signature_web_api.Models;
using bakery_web_api.Models;
using web_signature_web_api.Interfaces;

namespace web_signature_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User user)
        {
            return await _authService.Register(user);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            return await _authService.Login(loginRequest);
        }

        [HttpPost]
        [Route("VerifyToken")]
        public async Task<IActionResult> VerifyToken(TokenRequest tokenRequest)
        {
            return await _authService.VerifyToken(tokenRequest);
        }

        [HttpGet]
        [Route("User")]
        public async Task<IActionResult> GetUser([FromHeader] string userId)
        {
            return await _authService.GetUser(userId);
        }

        [HttpPost]
        [Route("UpdateData")]
        public async Task<IActionResult> UpdateUser(UserUpdateRequest userUpdateRequest)
        {
            return await _authService.UpdateUser(userUpdateRequest);
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            return await _authService.ChangePassword(changePasswordRequest);
        }
    }
}
using bakery_web_api.Models;
using Microsoft.AspNetCore.Mvc;
using web_signature_web_api.Models;

namespace web_signature_web_api.Interfaces
{
    public interface IAuthService
    {
        Task<IActionResult> Register(User user);
        Task<IActionResult> Login(LoginRequest loginRequest);
        Task<IActionResult> VerifyToken(TokenRequest tokenRequest);
        Task<IActionResult> GetUser(string userId);
        Task<IActionResult> UpdateUser(UserUpdateRequest userUpdateRequest);
        Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest);
    }
}

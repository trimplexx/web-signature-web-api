using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_signature_web_api.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using web_signature_web_api.Migrations;

namespace web_signature_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User user)
        {
            // Sprawdzenie, czy użytkownik o podanym adresie e-mail już istnieje
            if (await _context.Users.AnyAsync(x => x.Email == user.Email))
            {
                return BadRequest(new { error = "EmailAlreadyExists", message = "Użytkownik o podanym adresie e-mail już istnieje" });
            }

            // Sprawdzenie, czy hasło spełnia wymagane warunki
            if (!CheckPasswordRequirements(user.Password))
            {
                return BadRequest(new { error = "InvalidPassword", message = "Hasło musi mieć przynajmniej 6 znaków oraz zawierać dużą literę i znak specjalny." });
            }
            
            // Hashowanie hasła przed zapisaniem do bazy danych
            user.Password = HashPassword(user.Password);

            // Dodanie użytkownika do bazy danych
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Użytkownik został pomyślnie zarejestrowany" });
        }

        // Metoda do hashowania hasła
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        // Metoda do sprawdzania warunków hasła
        private bool CheckPasswordRequirements(string password)
        {
            if (password.Length < 6)
            {
                return false;
            }

            bool hasLowerChar = false;
            bool hasSpecialChar = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c))
                {
                    hasLowerChar = true;
                }
                else if (!char.IsLetterOrDigit(c))
                {
                    hasSpecialChar = true;
                }
            }

            return hasLowerChar && hasSpecialChar;
        }
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your_secret_key_here"); // Zastąp tym sekretnym kluczem
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Email", user.Email),
                    new Claim("FirstName", user.First_name) 
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Ustaw tu czas wygaśnięcia tokena
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginRequest.Email);
            if (user == null)
            {
                return NotFound(new { error = "UserNotFound", message = "Użytkownik o podanym adresie e-mail nie istnieje" });
            }

            var hashedPassword = HashPassword(loginRequest.Password);
            if (user.Password != hashedPassword)
            {
                return BadRequest(new { error = "InvalidCredentials", message = "Nieprawidłowe poświadczenia" });
            }

            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }

    }
}
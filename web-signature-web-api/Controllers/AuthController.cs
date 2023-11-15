﻿using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_signature_web_api.Models;
using System.Security.Cryptography;
using System.Text;
using bakery_web_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using web_signature_web_api.Migrations;

namespace web_signature_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User user)
        {
            // Sprawdzenie, czy użytkownik o podanym adresie e-mail już istnieje
            if (await _context.Users.AnyAsync(x => x.Email == user.Email))
            {
                return BadRequest(new
                    { error = "EmailAlreadyExists", message = "Użytkownik o podanym adresie e-mail już istnieje" });
            }

            user.Password = HashPassword(user.Password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Wystąpił błąd podczas rejestracji użytkownika." });
            }

            return Ok(new { message = "Użytkownik został pomyślnie zarejestrowany" });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginRequest.Email);
            if (user == null)
            {
                return NotFound(new
                    { error = "UserNotFound", message = "Użytkownik o podanym adresie e-mail nie istnieje" });
            }

            if (!CheckPassword(loginRequest.Password, user.Password))
            {
                return BadRequest(new { error = "InvalidCredentials", message = "Wprowadzono nieprawidłowe hasło" });
            }

            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }

        // Metoda do hashowania hasła
        private string HashPassword(string password)
        {
            // Generowanie soli
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            // Hashowanie hasła z solą
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Łączenie soli i hasha
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Zwracanie hasha jako stringa
            return Convert.ToBase64String(hashBytes);
        }

        // Metoda do sprawdzania warunków hasła
        private bool CheckPassword(string enteredPassword, string storedHash)
        {
            // Konwersja hasha do tablicy bajtów
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Pobieranie soli
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Hashowanie wprowadzonego hasła z solą
            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Sprawdzanie, czy hashe są takie same
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.Id.ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(72),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost]
        [Route("VerifyToken")]
        public IActionResult VerifyToken([FromBody] TokenRequest tokenRequest)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            try
            {
                tokenHandler.ValidateToken(tokenRequest.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return BadRequest(new { error = "InvalidToken", message = "Nieprawidłowy token" });
            }

            return Ok(new { message = "Token jest poprawny" });
        }

        [HttpGet]
        [Route("User")]
        public async Task<IActionResult> GetUser([FromHeader] string userId)
        {
            // Wyszukanie użytkownika w bazie danych
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Int32.Parse(userId));
            if (user == null)
            {
                return NotFound(new { error = "UserNotFound", message = "Nie znaleziono użytkownika" });
            }

            return Ok(new
            {
                first_name = user.First_name,
                last_name = user.Last_name,
                email = user.Email
            });
        }

        [HttpPost]
        [Route("UpdateData")]
        public async Task<IActionResult> UpdateUser(UserUpdateRequest userUpdateRequest)
        {
            // Wyszukanie użytkownika o podanym Id
            var user = await _context.Users.FindAsync(userUpdateRequest.Id);
            if (user == null)
            {
                return NotFound(new { error = "UserNotFound", message = "Nie znaleziono użytkownika" });
            }

            user.First_name = userUpdateRequest.First_name;
            user.Last_name = userUpdateRequest.Last_name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Wystąpił błąd podczas aktualizacji danych użytkownika." });
            }

            return Ok(new { message = "Dane użytkownika zostały pomyślnie zaktualizowane" });
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            // Wyszukanie użytkownika o podanym Id
            var user = await _context.Users.FindAsync(changePasswordRequest.Id);
            if (user == null)
            {
                return NotFound(new { error = "UserNotFound", message = "Nie znaleziono użytkownika" });
            }

            // Sprawdzenie, czy podane stare hasło jest poprawne
            if (!CheckPassword(changePasswordRequest.OldPassword, user.Password))
            {
                return BadRequest(new
                    { error = "InvalidCredentials", message = "Podane stare hasło jest nieprawidłowe" });
            }

            // Ustawienie nowego hasła
            user.Password = HashPassword(changePasswordRequest.NewPassword);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Wystąpił błąd podczas zmiany hasła." });
            }

            return Ok(new { message = "Hasło zostało pomyślnie zmienione" });
        }
    }
}
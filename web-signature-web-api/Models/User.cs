using System.ComponentModel.DataAnnotations;

namespace web_signature_web_api.Models
{
    public class User
    {
        [Key] public int Id { get; set; }

        public string First_name { get; set; }

        public string Last_name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
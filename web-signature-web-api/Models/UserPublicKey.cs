using System.ComponentModel.DataAnnotations;

namespace web_signature_web_api.Models
{
    public class UserPublicKey
    {
        [Key]   
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PublicKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; } 

    }
}

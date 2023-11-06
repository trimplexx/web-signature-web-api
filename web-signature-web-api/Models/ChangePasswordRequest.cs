namespace web_signature_web_api.Models;

public class ChangePasswordRequest
{
    public int Id { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
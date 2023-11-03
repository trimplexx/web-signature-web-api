namespace web_signature_web_api.Models;

public class EditRequest
{
    public int Id { get; set; }
    public string First_name { get; set; }
    public string Last_name { get; set; }
    public string Old_password { get; set; }
    public string New_password { get; set; }
}
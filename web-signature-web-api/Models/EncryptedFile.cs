namespace web_signature_web_api.Models;

public class EncryptedFile
{
    public string EncryptedPdfFileBase64 { get; set; }
    public string EncryptedAESKeyBase64 { get; set; }
    public string PrivateKeyFileBase64 { get; set; }
}
using Microsoft.AspNetCore.Mvc;
using web_signature_web_api.Models;

namespace web_signature_web_api.Interfaces
{
    public interface IAESencryptionService
    {
        Task<IActionResult> AESencrypt(FileUpload fileUpload);
    }
}

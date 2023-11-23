using Microsoft.AspNetCore.Mvc;
using web_signature_web_api.Models;

namespace web_signature_web_api.Interfaces
{
    public interface ICryptoService
    {
        Task<IActionResult> UploadPublicKey(PublicKeyRequest request);
        Task<IActionResult> CheckKeyExists(int userId);
        Task<IActionResult> DeleteKey(int id);
        Task<IActionResult> DownloadPublicKey(int userId);
    }
}

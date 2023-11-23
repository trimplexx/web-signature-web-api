using Microsoft.AspNetCore.Mvc;
using web_signature_web_api.Interfaces;
using web_signature_web_api.Models;


namespace web_signature_web_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly ICryptoService _cryptoService;

        public CryptoController(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        [HttpPost("upload-public-key")]
        public async Task<IActionResult> UploadPublicKey([FromBody] PublicKeyRequest request)
        {
            return await _cryptoService.UploadPublicKey(request);
        }

        [HttpGet("check-key-exists")]
        public async Task<IActionResult> CheckKeyExists([FromQuery] int userId)
        {
            return await _cryptoService.CheckKeyExists(userId);
        }

        [HttpDelete("delete-key")]
        public async Task<IActionResult> DeleteKey([FromQuery] int id)
        {
            return await _cryptoService.DeleteKey(id);
        }

        [HttpGet("download-public-key/{userId}")]
        public async Task<IActionResult> DownloadPublicKey(int userId)
        {
            return await _cryptoService.DownloadPublicKey(userId);
        }
    }
}
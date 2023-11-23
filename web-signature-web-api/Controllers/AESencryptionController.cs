using Microsoft.AspNetCore.Mvc;
using web_signature_web_api.Interfaces;
using web_signature_web_api.Models;

namespace web_signature_web_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AESencryptionController : ControllerBase
    {
        private readonly IAESencryptionService _aesEncryptionService;

        public AESencryptionController(IAESencryptionService aesEncryptionService)
        {
            _aesEncryptionService = aesEncryptionService;
        }

        [HttpPost("aes-encrypt")]
        public async Task<IActionResult> AESencrypt([FromBody] FileUpload fileUpload)
        {
            return await _aesEncryptionService.AESencrypt(fileUpload);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using MfaSystemDemo.Models;      // Imports our models namespace
using MfaSystemDemo.Services;    // Imports our services namespace

namespace MfaSystemDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MfaService _mfaService;
        private static readonly ConcurrentDictionary<string, UserDbRecord> MockUserDb = new();

        public AuthController(MfaService mfaService)
        {
            _mfaService = mfaService;
        }

        [HttpPost("setup")]
        public IActionResult SetupMfa([FromBody] string email)
        {
            var (base32Secret, _) = _mfaService.GenerateSecret();
            string qrCodeUrl = _mfaService.GenerateQrCodeUrl(email, base32Secret);

            MockUserDb[email] = new UserDbRecord { Email = email, SecretKey = base32Secret, IsMfaEnabled = false };

            return Ok(new
            {
                Secret = base32Secret,
                QrCodeUri = qrCodeUrl
            });
        }

        [HttpPost("verify-setup")]
        public IActionResult VerifySetup([FromBody] VerifyRequest request)
        {
            if (!MockUserDb.TryGetValue(request.Email, out var user))
                return BadRequest("User not found.");

            bool isValid = _mfaService.VerifyCode(user.SecretKey, request.Code);

            if (isValid)
            {
                user.IsMfaEnabled = true;
                return Ok(new { Message = "MFA successfully enabled!" });
            }

            return BadRequest("Invalid validation code. Try again.");
        }

        [HttpPost("login-mfa")]
        public IActionResult LoginMfa([FromBody] VerifyRequest request)
        {
            if (!MockUserDb.TryGetValue(request.Email, out var user))
                return BadRequest("User not found.");

            if (!user.IsMfaEnabled)
                return BadRequest("MFA is not enabled for this account.");

            bool isValid = _mfaService.VerifyCode(user.SecretKey, request.Code);

            if (isValid)
            {
                return Ok(new { Token = "mock-secure-jwt-token-granted", Status = "Authenticated" });
            }

            return Unauthorized("Invalid MFA code.");
        }
        // Endpoint 1B: Returns a scannable PNG QR code image directly to the browser
        [HttpGet("setup-qr-image")]
        public IActionResult GetMfaQrImage([FromQuery] string email)
        {
            // 1. Check if the user went through Step 1 (/setup) first
            if (!MockUserDb.TryGetValue(email, out var user))
            {
                return BadRequest("User not found. Please call the /setup endpoint first to generate a secret.");
            }

            // 2. Re-create the URL using their saved secret key
            string qrCodeUrl = _mfaService.GenerateQrCodeUrl(email, user.SecretKey);

            // 3. Generate the graphic byte array
            byte[] qrCodeImageBytes = _mfaService.GenerateQrCodeImage(qrCodeUrl);

            // 4. Stream it directly to the browser window as an image file
            return File(qrCodeImageBytes, "image/png");
        }

    }
}

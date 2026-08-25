using OtpNet;
using System.Text;
using Microsoft.Extensions.Configuration;
using QRCoder;


namespace MfaSystemDemo.Services
{
    public class MfaService
    {
        private readonly string _issuer;

        public MfaService(IConfiguration configuration)
        {
            _issuer = configuration["MfaSettings:Issuer"] ?? "DefaultApp";
        }

        public (string Base32Secret, string RawSecret) GenerateSecret()
        {
            byte[] secretBytes = KeyGeneration.GenerateRandomKey(20);
            string base32Secret = Base32Encoding.ToString(secretBytes);
            string rawSecret = Encoding.UTF8.GetString(secretBytes);
            return (base32Secret, rawSecret);
        }

        public string GenerateQrCodeUrl(string userEmail, string base32Secret)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(_issuer)}:{Uri.EscapeDataString(userEmail)}?secret={base32Secret}&issuer={Uri.EscapeDataString(_issuer)}";
        }

        public bool VerifyCode(string base32Secret, string code)
        {
            try
            {
                byte[] secretBytes = Base32Encoding.ToBytes(base32Secret);
                var totp = new Totp(secretBytes, step: 30);

                long timeStepMatched = 0;
                //bool isValid = totp.VerifyTotp(code, out timeStepMatched, VerificationWindow.Standard);
                bool isValid = totp.VerifyTotp(code, out timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);


                return isValid;
            }
            catch
            {
                return false;
            }
        }
        public byte[] GenerateQrCodeImage(string qrCodeUrl)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                // 1. Generate the raw QR data matrix from the URL
                using (var qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q))
                {
                    // 2. Render the matrix into a clean PNG image
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        // Returns raw PNG image bytes (pixelsPerModule: 20 makes it look sharp)
                        return qrCode.GetGraphic(20);
                    }
                }
            }
        }
    }
}

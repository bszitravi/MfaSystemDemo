namespace MfaSystemDemo.Models
{
    public class UserDbRecord
    {
        public string Email { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool IsMfaEnabled { get; set; }
    }
   
        public class VerifyRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }
  

}

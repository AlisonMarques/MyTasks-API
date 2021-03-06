using System;

namespace MyTasks_API.V1.Models
{
    public class TokenDto
    {
        public string Token { get; set; }
        
        public DateTime Expiration { get; set; }
        
        public string RefreshToken { get; set; }
        
        public DateTime ExpirationRefreshToken { get; set; }
    }
}
namespace ProjectEXE.Views.Account
{
    public class UserSessionInfo
    {
        public int UserId { get; set; }           
        public string SessionId { get; set; }     
        public string UserAgent { get; set; }     
        public string IpAddress { get; set; }     
        public DateTime CreatedAt { get; set; }   
        public DateTime LastActivity { get; set; } 
        public bool IsActive { get; set; }        
    }
}

namespace hub.Models
{
    public class UserFile
    {
        public int Id { get; set; }
        public string? FilePath { get; set; }  
        public long FileSize { get; set; }     
        public string? OriginalFilename { get; set; }  
        public bool Processed { get; set; }    
        public DateTime UploadedAt { get; set; } 

        public int UserId { get; set; }  
        public User? User { get; set; }  
    }
}

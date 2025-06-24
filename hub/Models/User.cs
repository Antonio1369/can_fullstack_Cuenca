namespace hub.Models
{   public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public ICollection<UserQuery>? UserQueries { get; set; }

    }
}
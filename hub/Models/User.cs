using System.ComponentModel.DataAnnotations.Schema;

namespace hub.Models
{
    [Table("user", Schema = "public")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("is_superuser")]
        public bool IsSuperuser { get; set; }

        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("is_staff")]
        public bool IsStaff { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("date_joined")]
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;

        public ICollection<UserQuery>? UserQueries { get; set; }
        public ICollection<UserFile>? UserFiles { get; set; }
    }
}
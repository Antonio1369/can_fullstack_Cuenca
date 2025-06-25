using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace hub.Models
{
    [Table("user_files", Schema = "public")]
    public class UserFile
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column("filename")]
        public string Filename { get; set; } = string.Empty;

        [Column("task_id")]
        public string? TaskId { get; set; }

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Column("processed")]
        public bool Processed { get; set; } = false;
    }
}

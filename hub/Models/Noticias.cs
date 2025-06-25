using System.ComponentModel.DataAnnotations.Schema;

namespace hub.Models
{
    [Table("noticias")]
    public class Noticias
    {
        [Column("id")]
        public int Id { get; set; } 
        
        [Column("url")]
        public string? Url { get; set; }
        
        [Column("title")]
        public string? Title { get; set; }
        
        [Column("content")]
        public string? Content { get; set; }
        
        [Column("embedding")]
        public string? Embedding { get; set; }

        public override string? ToString() => Title;
    }
}
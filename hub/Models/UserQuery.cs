using System.ComponentModel.DataAnnotations.Schema;

namespace hub.Models
{
    [Table("userquery")]
    public class UserQuery
    {
        [Column("id")]
        public int Id { get; set; }  
        
        [Column("query_text")]
        public string? QueryText { get; set; }  
        
        [Column("query_embedding")]
        public string? QueryEmbedding { get; set; }  
        
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }  

        [Column("user_id")]
        public int UserId { get; set; }  // llave foránea
        
        public User? User { get; set; }  

        public void SaveEmbedding(string? embedding)
        {
            QueryEmbedding = embedding;
        }

        public string? GetEmbedding()
        {
            return QueryEmbedding;
        }
    }
}
namespace hub.Models
{
    public class UserQuery
    {
        public int Id { get; set; }  
        public string? QueryText { get; set; }  
        public string? QueryEmbedding { get; set; }  
        public DateTime? CreatedAt { get; set; }  

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

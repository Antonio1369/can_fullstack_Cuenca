namespace hub.Models
{
    public class Noticias
    {
        public string? Url { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Embedding { get; set; }

        public override string? ToString() => Title;  // Cambié a string? para permitir null
    }
}

namespace AiContextModels
{
    public class Blog
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string Image { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
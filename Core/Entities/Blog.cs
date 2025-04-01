namespace BlogProject.Core.Entities
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Summary { get; set; }
        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

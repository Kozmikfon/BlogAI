
namespace BlogProject.Core.Entities
{
    public class GeneratedBlog
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? Tags { get; set; }
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = "Yapay Zeka";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
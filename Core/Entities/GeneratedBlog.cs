
namespace BlogProject.Core.Entities
{
    public class GeneratedBlog
    {
        public int Id { get; set; } //  ID eklendi
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Tags { get; set; }
        //public string Category { get; set; }
        public DateTime CreatedAt { get; internal set; }
    }
}

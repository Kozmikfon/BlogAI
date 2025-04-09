namespace BlogProject.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int BlogId { get; set; }
        public Blog? Blog { get; set; } = null!;
    }

}
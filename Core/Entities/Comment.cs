namespace BlogProject.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int BlogId { get; set; }

        // Entity Framework ilişki için (opsiyonel)
        public Blog? Blog { get; set; }
    }
}

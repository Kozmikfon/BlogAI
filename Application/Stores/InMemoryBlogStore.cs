using BlogProject.Core.Entities;

public class InMemoryBlogStore
{
    private readonly List<GeneratedBlog> _blogs = new();
    private int _nextId = 1;
    public void Add(GeneratedBlog blog)
    {
        blog.Id = ++_nextId;
        blog.CreatedAt = DateTime.Now;
        _blogs.Add(blog);
    }

    public List<GeneratedBlog> GetAll() => _blogs;

    public GeneratedBlog? GetById(int id)
    {
        return _blogs.FirstOrDefault(b => b.Id == id); // 🔥 ID ile eşleşen blogu getir
    }
}

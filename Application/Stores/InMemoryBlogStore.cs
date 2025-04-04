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

    public List<GeneratedBlog> GetAll()
    {
        return _blogs.OrderByDescending(b => b.Id).ToList();
    }

    public GeneratedBlog? GetById(int id)
    {
        return _blogs.FirstOrDefault(b => b.Id == id); //  ID ile eşleşen blogu getir
    }
    public bool Delete(int id)
    {
        var blog = _blogs.FirstOrDefault(b => b.Id == id);
        if (blog == null)
            return false;

        _blogs.Remove(blog);
        return true;
    }
    public bool Update(int id, GeneratedBlog updated)
    {
        var blog=_blogs.FirstOrDefault(b => b.Id == id);

        if (blog == null) return false;

        
        blog.Title = updated.Title;
        blog.Summary = updated.Summary;
        blog.Content = updated.Content;
        blog.ImageUrl = updated.ImageUrl;
        blog.Tags = updated.Tags;
        //blog.Category= updated.Category;

        return true;
    }
    public List<string> GetRecentTitles(int count = 10)
    {
        return _blogs
            .OrderByDescending(b => b.CreatedAt)
            .Take(count)
            .Select(b => b.Title ?? "")
            .ToList();
    }



}

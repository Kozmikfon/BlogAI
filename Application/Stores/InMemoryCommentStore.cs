using BlogProject.Core.Entities;

namespace BlogProject.Application.Stores
{
    public class InMemoryCommentStore
    {
        private readonly List<Comment> _comments = new();
        private int _nextId = 1;

        public void Add(Comment comment)
        {
            comment.Id = _nextId++;
            comment.CreatedAt = DateTime.Now;
            _comments.Add(comment);
        }

        public List<Comment> GetByBlogId(int blogId)
        {
            return _comments.Where(c => c.BlogId == blogId).ToList();
        }

        public List<Comment> GetAll()
        {
            return _comments.OrderByDescending(c => c.CreatedAt).ToList();
        }

        public bool Delete(int id)
        {
            var comment = _comments.FirstOrDefault(c => c.Id == id);
            if (comment == null) return false;
            _comments.Remove(comment);
            return true;
        }
    }
}

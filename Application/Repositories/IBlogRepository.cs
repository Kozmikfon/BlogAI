using BlogProject.Core.Entities;

namespace BlogProject.Application.Repositories
{
    public interface IBlogRepository
    {
        Task<List<GeneratedBlog>> GetAllAsync();
        Task<GeneratedBlog?> GetByIdAsync(int id);
        Task AddAsync(GeneratedBlog blog);
        Task DeleteAsync(int id);
    }
}

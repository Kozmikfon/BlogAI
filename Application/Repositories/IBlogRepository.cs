using BlogProject.Core.Entities;

namespace BlogProject.Application.Repositories
{
    public interface IBlogRepository
    {
        Task<List<Blog>> GetAllAsync();
        Task<Blog?> GetByIdAsync(int id);
        Task AddAsync(Blog blog);
        Task DeleteAsync(int id);
    }
}

using BlogProject.Application.Agents;
using BlogProject.Infrastructure.Data;
using BlogProject.Core.Entities;


namespace BlogProject.BackgroundJobs
{
    public class BlogAgentJob
    {
        private readonly BlogAgentService _agent;
        private readonly BlogDbContext _db;


        public BlogAgentJob(BlogAgentService agent, BlogDbContext db)
        {
            _agent = agent;
            _db = db;
        }

        public async Task GenerateScheduledBlog(string time)
        {
            string[] categories = { "Teknoloji", "Bilim", "Sağlık", "Girişimcilik", "Yapay Zeka" };
            var dayIndex = DateTime.Now.Day % categories.Length;
            var category = categories[dayIndex];

            var blog = await _agent.GenerateSmartBlogAsync(category);

            if (blog != null)
            {
                blog.Category = category;
                _db.Add(blog);
                Console.WriteLine($"✅ {time} için blog eklendi: {blog.Title}");
            }
        }
    }
}

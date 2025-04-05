using BlogProject.Application.Agents;
using BlogProject.Application.Stores;
using BlogProject.Core.Entities;

namespace BlogProject.BackgroundJobs
{
    public class BlogAgentJob
    {
        private readonly BlogAgentService _agent;
        private readonly InMemoryBlogStore _store;

        public BlogAgentJob(BlogAgentService agent, InMemoryBlogStore store)
        {
            _agent = agent;
            _store = store;
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
                _store.Add(blog);
                Console.WriteLine($"✅ {time} için blog eklendi: {blog.Title}");
            }
        }
    }
}

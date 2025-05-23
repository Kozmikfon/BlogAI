﻿using System.ComponentModel.DataAnnotations.Schema;

namespace BlogProject.Core.Entities
{
    [Tags("Blogs")]
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Comment> Comments { get; set; } = new();
        


    }
}
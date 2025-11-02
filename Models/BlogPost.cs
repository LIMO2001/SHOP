using System;
using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public required string Title { get; set; }

        [Required]
        [StringLength(255)]
        public string? Slug { get; set; }

        [Required]
        public required string Content { get; set; }

        [StringLength(255)]
        public required string ImageUrl { get; set; }

        [Required]
        [StringLength(100)]
        public required string Author { get; set; }

        public DateTime DatePosted { get; set; }

        public bool IsPublished { get; set; }
    }
}
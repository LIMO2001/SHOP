using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LaptopStore.Services;

namespace LaptopStore.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _blogService.GetPublishedPostsAsync();
            return View(posts);
        }

        public async Task<IActionResult> Post(string slug)
        {
            var post = await _blogService.GetPostBySlugAsync(slug);
            if (post == null)
            {
                return NotFound();
            }

            // Get recent posts for sidebar
            ViewBag.RecentPosts = await _blogService.GetRecentPostsAsync(5);
            return View(post);
        }
    }
}
using AiContextModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_app_ai_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        AiContext _aicontext;
        IWebHostEnvironment _env;
        List<string> _extentions = new List<string>(){".jpg", ".jpeg", ".png"};

        public PostsController(AiContext aicontext, IWebHostEnvironment env)
        {
            _aicontext = aicontext;
            _env = env;
        }

        [HttpGet]
        public IActionResult GetAllPosts()
        {
            List<Blog> blogs = _aicontext.Blogs.ToList();
            return Ok(blogs);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromForm] BlogDTO blogdto)
        {
            string path = Path.Combine(_env.ContentRootPath, "wwwroot/blogs");

            string ext = Path.GetExtension(blogdto.Image.FileName);
            if(!_extentions.Contains(ext))return BadRequest();
            string fileName = $"{Guid.NewGuid()}{ext}";
            string filePath = Path.Combine(path, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await blogdto.Image.CopyToAsync(stream);
            }
            Blog blog = new Blog()
            {
                Title = blogdto.Title,
                Content = blogdto.Content,
                Image = "blogs/"+fileName,
                CreatedAt = DateTime.Now
            };
            _aicontext.Blogs.Add(blog);
            _aicontext.SaveChanges();
            return Ok(blog);
        }
    }

    public class BlogDTO
    {
        public required string Title {get; set;}
        public required string Content {get; set;}
        public required IFormFile Image {get; set;}
    }

}
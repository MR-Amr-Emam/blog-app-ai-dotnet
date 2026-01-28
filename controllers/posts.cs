using AiContextModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog_app_ai_dotnet.models;

using CreateAiBlog;




namespace blog_app_ai_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        AiContext _aicontext;
        DefaultContext _usercontext;
        IWebHostEnvironment _env;
        IBlogAiService _blogAiService;
        string _origin;

        List<string> _extentions = new List<string>(){".jpg", ".jpeg", ".png"};

        public PostsController(AiContext aicontext, DefaultContext usercontext, IWebHostEnvironment env, IConfiguration config,
        IBlogAiService blogAiService)
        {
            _aicontext = aicontext;
            _usercontext = usercontext;
            _env = env;
            _blogAiService = blogAiService;
            _origin = config.GetValue<string>("AppSettings:BaseOrigin") ?? "http://localhost:5028";

        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var blogsDb = _aicontext.Blogs;
            var blogsDto = new List<BlogDto>();
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value??"0");

            foreach(Blog blog in blogsDb)
            {
                BlogDto blogDto = new BlogDto()
                {
                    Id=blog.Id,
                    Title=blog.Title,
                    Description=blog.Content,
                    Image=$"{_origin}/{blog.Image}",
                    Likes=blog.Likes,
                    Views=blog.Views,
                    Date=blog.CreatedAt,
                    Liked=blog.People_liked.Contains(userId)
                };
                blogsDto.Add(blogDto);
            }
            return Ok(blogsDto);

        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetPost(int id)
        {
            Blog? blog = await _aicontext.Blogs.SingleOrDefaultAsync((b)=>b.Id==id);
            if(blog==null)return NotFound();
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value??"0");
            if (!blog.People_viewed.Contains(userId))
            {
                blog.People_viewed.Add(userId);
                blog.Views += 1;
            }
            await _aicontext.SaveChangesAsync();
            BlogDto blogDto = new BlogDto()
                {
                    Id=blog.Id,
                    Title=blog.Title,
                    Description=blog.Content,
                    Image=$"{_origin}/{blog.Image}",
                    Likes=blog.Likes,
                    Views=blog.Views,
                    Date=blog.CreatedAt,
                    LikedPeople=blog.People_liked,
                    ViewedPeople=blog.People_viewed,
                    Liked=blog.People_liked.Contains(userId),
                };
            return Ok(blogDto); 
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromForm] CreateBlogDTO blogdto)
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

        [Authorize(Roles = "admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                await _aicontext.Blogs.ExecuteDeleteAsync();
                return Ok();
            }
            catch{
                return BadRequest();
            }

        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> AddLike(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value??"0");
            Blog? blog = await _aicontext.Blogs.SingleOrDefaultAsync(b => b.Id==id);
            if(blog==null)return BadRequest();

            if(blog.People_liked.Contains(userId))blog.RemoveLike(userId);
            else blog.AddLike(userId);

            await _aicontext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("createaiblog")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateAiBlog()
        {
            AiBlog? blogData = await _blogAiService.CreateAiBlog();
            if(blogData==null)return Problem(
                detail: "A database timeout occurred while processing your request.",
                title: "Service Failure, because of Ai el3ars",
                statusCode: StatusCodes.Status500InternalServerError
            );
            Blog blog = new Blog()
            {
                Title = blogData.Title,
                Content = blogData.Content,
                Image = blogData.Image,
                CreatedAt = DateTime.Now
            };
            _aicontext.Blogs.Add(blog);
            _aicontext.SaveChanges();
            return Ok(blog);
        }
    }

    public class BlogDto
    {
        public int Id {get; set;}
        public required string Title {get; set;}
        public required string Description {get; set;}
        public required string Image {get; set;}
        public int Views {get; set;}
        public int Likes {get; set;}
        public required DateTime Date {get; set;}
        public List<int>? ViewedPeople {get; set;}
        public List<int>? LikedPeople {get; set;}
        public bool Liked {get;set;}
    }
    public class CreateBlogDTO
    {
        public required string Title {get; set;}
        public required string Content {get; set;}
        public required IFormFile Image {get; set;}
    }

}
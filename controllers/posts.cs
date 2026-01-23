using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_app_ai_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllPosts()
        {
            // TODO: Inject your service and retrieve posts from database
            var posts = new List<PostDto>();
            PostDto post = new PostDto{Id=1, Content="hello", AuthorName="amr", CreatedAt=DateTime.Now, LikeCount=0, CommentCount=0};
            posts.Add(post);
            return Ok(posts);    
        }
    }

    public class PostDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}
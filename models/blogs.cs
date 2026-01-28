using System.ComponentModel.DataAnnotations.Schema;
using blog_app_ai_dotnet.models;
namespace AiContextModels
{
    public class Blog
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string Image { get; set; }
        public int Views {get; set;} = 0;
        public int Likes {get; set;} = 0;
        public DateTime CreatedAt { get; set; }

        public List<int> People_liked {get; set;} = new List<int>();
        public List<int> People_viewed {get; set;} = new List<int>();
    }

    public static class BlogExtension
    {
        public static void AddLike(this Blog blog, int userId)
        {
            blog.People_liked.Add(userId);
            blog.Likes += 1;
        }

        public static void RemoveLike(this Blog blog, int userId)
        {
            blog.People_liked.Remove(userId);
            blog.Likes -= 1;
        }
    }

}
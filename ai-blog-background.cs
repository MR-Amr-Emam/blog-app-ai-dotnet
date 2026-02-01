using AiContextModels;
using CreateAiBlog;

namespace backgroundServices
{
    public class AiBlogBG : BackgroundService
    {
        PeriodicTimer _timer;
        Func<Task<AiBlog?>> _createAiBlogAsync;
        IServiceScopeFactory _serviceScopeFactory;
        public AiBlogBG(IBlogAiService blogAiService, IServiceScopeFactory serviceScopeFactory)
        {
            _createAiBlogAsync = blogAiService.CreateAiBlog;
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(3));
            _serviceScopeFactory = serviceScopeFactory;

        }
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while(!token.IsCancellationRequested && (await _timer.WaitForNextTickAsync(token)))
            {
                Console.WriteLine("Entered BackGround Service *************");
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                AiContext context = scope.ServiceProvider.GetRequiredService<AiContext>();
                DateTime lastAdded = context.Blogs.OrderBy(b=>b.CreatedAt).Select(b=>b.CreatedAt).Last();
                if(lastAdded.Date == DateTime.Now.Date)continue;

                AiBlog? aiBlog = null;
                while (aiBlog == null)aiBlog = await _createAiBlogAsync();

                Console.WriteLine(aiBlog);
                Blog blog = new Blog()
                {
                    Title = aiBlog.Title,
                    Content = aiBlog.Content,
                    Image = aiBlog.Image,
                    CreatedAt = DateTime.Now
                };
                context.Blogs.Add(blog);
                await context.SaveChangesAsync();
            }
        }

    }
}
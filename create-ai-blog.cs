using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;


namespace CreateAiBlog
{
    public interface IBlogAiService
    {
        public Task<AiBlog?> CreateAiBlog();
    }
    

    public class BlogAiService(IConfiguration _config):IBlogAiService{

        public async Task<AiBlog?> CreateAiBlog(){
            string key = Environment.GetEnvironmentVariable("AiBlogToken")??"";
            string model = _config["OpenRouter:model"]??"";
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            List<Dictionary<string, string>> messages = [
                new Dictionary<string, string>()
                {
                    {"role", "system"},
                    {"content",
                    """
                        your task is to generate a valid json string for a post its content depends on the user ask,
                        the json object should include only these keys, (the data should be actually true and real)
                        1) 'blogTitle'
                        2) 'blogDescription'
                        3) 'blogImage' that is remote image link about the post content make sure the link is actually working
                    """}
                },
                new Dictionary<string, string>(){
                    {"role", "user"},
                    {"content", "make a post about the most trending event that week"}
                }
            ];

            var requestBody = new
            {
                model,
                messages
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_config["OpenRouter:remoteurl"]??"", content);

            if (!response.IsSuccessStatusCode){
                Console.WriteLine(response.StatusCode.ToString());
                return null;
            };

            string responseJson = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(responseJson);
            string jsonString = doc.RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content").GetString()??"";
            Console.WriteLine(jsonString);
            try
            {
                jsonString = JsonDetect(jsonString);   
            }catch(Exception e){
                Console.WriteLine(e.Message);
                return null;
            }
            JsonElement blogJson = JsonDocument.Parse(jsonString).RootElement;
           
            string imagePath = await DownloadImage(blogJson.GetProperty("blogImage").GetString()??"");
            if(imagePath == "")return null;

            return new AiBlog()
            {
                Title = blogJson.GetProperty("blogTitle").GetString() ?? "",
                Content = blogJson.GetProperty("blogDescription").GetString() ?? "",
                Image = imagePath
            };
            
        }

        static private string JsonDetect(string str)
        {
            int[] indecies = {str.IndexOf('{'), str.LastIndexOf('}')};
            return str.Substring(indecies[0], indecies[1]-indecies[0]+1);
        }

        static private async Task<string> DownloadImage(string imageUrl)
        {
            HttpClient httpClient = new HttpClient();
            string imagePath;
            List<string> allowedExtensions = ["jpeg", "jpg", "png"];
            try
            {
            // Get the image as a stream
                using (HttpResponseMessage httpResponse = await httpClient.GetAsync(imageUrl))
                {
                    string extension = httpResponse.Content.Headers.ContentType?.MediaType??"";
                    extension = extension.Substring(extension.LastIndexOf('/')+1);
                    Console.WriteLine(extension);
                    if(!allowedExtensions.Contains(extension)){
                        Console.WriteLine("invalid extension");
                        return "";
                    }
                    Guid imageId = Guid.NewGuid();
                    imagePath = $"blogs/{imageId}.{extension}";
                    Stream imageStream = await httpResponse.Content.ReadAsStreamAsync();
                    // Create a FileStream to write the data to a local file
                    FileStream fileStream = new FileStream(Path.Combine("wwwroot", imagePath), FileMode.Create, FileAccess.Write);
                    await imageStream.CopyToAsync(fileStream);
                    //Console.WriteLine($"Image successfully downloaded to: {destinationPath}");
                }
                return imagePath;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"An error occurred during the HTTP request: {ex.Message}");
                return "";
            }
        }
    }

    public class AiBlog
    {
        public required string Title {get; set;}
        public required string  Content  {get; set;}
        public required string Image {get; set;}

        public override string ToString(){
            return $"{this.Title} \n \n {this.Content} \n \n {this.Image}";
        }
    }


}


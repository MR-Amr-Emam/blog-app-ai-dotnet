using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;


namespace CreateAiBlog
{
    public interface IBlogAiService
    {
        public Task<AiBlog?> CreateAiBlog();
    }
    

    public class BlogAiService(IConfiguration _config):IBlogAiService{

        public async Task<AiBlog?> CreateAiBlog(){
            string key = _config["OpenRouter:token"]??"";
            string model = _config["OpenRouter:model"]??"";
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            List<Dictionary<string, string>> messages = [
                new Dictionary<string, string>()
                {
                    {"role", "system"},
                    {"content",
                    """
                        your task is to generate json string for a post its content depends on the user ask,
                        the json object should include only these keys
                        1) 'blogTitle'
                        2) 'blogDescription'
                        3) 'blogImage' which is a remote url image related to post content
                    """}
                },
                new Dictionary<string, string>(){
                    {"role", "user"},
                    {"content", "make a post about trending event that day"}
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

            if (!response.IsSuccessStatusCode)return null;

            string responseJson = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(responseJson);
            string jsonString = doc.RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content").GetString()??"";
            jsonString = JsonDetect(jsonString);
            Console.WriteLine(jsonString);
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
                    string extention = httpResponse.Content.Headers.ContentType?.MediaType??"";
                    extention = extention.Substring(extention.LastIndexOf('/')+1);
                    if(!allowedExtensions.Contains(extention))return "";
                    Guid imageId = Guid.NewGuid();
                    imagePath = $"blogs/{imageId}.{extention}";
                    Stream imageStream = await httpResponse.Content.ReadAsStreamAsync();
                    // Create a FileStream to write the data to a local file
                    FileStream fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write);
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
    }


}


using Newtonsoft.Json;
using System.Text;

namespace DiscordBot.Config
{
    public class JSONReader
    {
        public string token { get; set; }
        public string prefix { get; set; }

        public async Task ReadJSON() 
        {
            using (StreamReader reader = new StreamReader("appConfig.json", new UTF8Encoding())) 
            {
                string json = await reader.ReadToEndAsync();
                ConfigJSON obj = JsonConvert.DeserializeObject<ConfigJSON>(json);

                this.token = obj.token;
                this.prefix = obj.prefix;
            }
        }
    }

    internal sealed class ConfigJSON
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}

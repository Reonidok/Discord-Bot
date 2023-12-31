using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.Utils
{
    public struct ConfigJson
    {
        [JsonProperty("discordToken")]
        public string DiscordToken { get; private set; }

        [JsonProperty("yandexMusicToken")]
        public string YandexToken { get; private set; }

        [JsonProperty("commandPrefix")]
        public string CommandPrefix { get; private set; }

        [JsonProperty("yandexMusicLogin")]
        public string Login { get; private set; }

        [JsonProperty("yandexMusicPassword")]
        public string Password { get; private set; }

        [JsonProperty("downloadPath")]
        public string DownloadPath { get; private set; }
    }
}

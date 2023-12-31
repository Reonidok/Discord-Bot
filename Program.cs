using System.Text;
using DSharpPlus;
using Newtonsoft.Json;
using DiscordBot.Bot.Utils;
using DiscordBot.Commands;
using DiscordBot.Config;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;

namespace DiscordBot 
{
    public sealed class Program
    {
        public static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        static async Task Main(string[] args)
        {
            var configJson = new JSONReader();
            await configJson.ReadJSON();


            var config = new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = configJson.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };
            
            Client = new DiscordClient(config);
            
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });
            
            Client.Ready += OnClientReady;
            
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };


            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<RegularCommands>();

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "0.0.0.0",
                Port = 44,
                Secured = false
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "pswrd",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Client.UseLavalink();
            await Client.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            Console.WriteLine($"\u001b[32mSUCC\u001b[0m Successfully was connected to {endpoint.Hostname}:{endpoint.Port} and bot token {configJson.token}");
            await Task.Delay(-1);
        }

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}

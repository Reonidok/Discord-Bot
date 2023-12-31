using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace DiscordBot.Commands
{
    public class RegularCommands : BaseCommandModule
    {
        private static int _next = 0;
        private List<LavalinkTrack> _queue = new List<LavalinkTrack>();
        
        [Command("test")]
        public async Task TestCommand(CommandContext ctx) 
        {
            await Task.Delay(100);
            var id = ctx.Guild.Id;
            await ctx.Channel.SendMessageAsync($"Test msg on {id}");
        }

        [Command("play")]
        public async Task PlayTrack(CommandContext ctx, [RemainingText]string query)
        {
            
            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            if(ctx.Member.VoiceState == null || userVC == null) 
            {
                await ctx.Channel.SendMessageAsync("Please enter a VC!");
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection is not established!");
                return;
            }

            if(userVC.Type != ChannelType.Voice) 
            {
                await ctx.Channel.SendMessageAsync("Please enter a valid VC");
                return;
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(userVC);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null) 
            {
                await ctx.Channel.SendMessageAsync("Failed to connect to the dedicated server");
                return;
            }
            
            var searchQuery = await node.Rest.GetTracksAsync(query, LavalinkSearchType.Youtube);
            if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches
                || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) 
            {
                await ctx.Channel.SendMessageAsync($"Failed to find music with query: {query}");
                return;
            }
            _queue.Add(searchQuery.Tracks.First());
            
            if (_queue.Count == 1)
            {
                await conn.PlayAsync(_queue.First());
            }
            
            conn.PlaybackFinished += async (sender, args) =>
            {
                if (args.Reason == TrackEndReason.Finished)
                {
                    _next = (_next + 1) % (_queue.Count - 1);
                    if (_queue.Any()) 
                        await sender.PlayAsync(_queue[_next]);
                }
            };
            
            _next = (_next + 1) % _queue.Count;
            var playTrack = _queue.Any() ? _queue[_next].Title : "there is nothing to play";
            await conn.SetVolumeAsync(100);
            string musicDescription = $"Now playing: {_queue.First().Title} \n"
                                      + $"Next will be played: {playTrack} \n"
                                      + $"Author: {_queue.First().Author} \n"
                                      + $"URL: {_queue.First().Uri} \n"
                                      + $"List size: {_queue.Count}";

            var nowPlayingEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Azure,
                Title = $"Successfully joined channel {userVC.Name} and playing music",
                Description = musicDescription
            };

            await ctx.Channel.SendMessageAsync(embed: nowPlayingEmbed);

            Console.WriteLine($"Now is track {_queue.First().Title} on AIR {DateTime.Now} with name: {playTrack}, by {ctx.Member.Nickname}: {ctx.Member.Color.R}");
            await Task.Delay(-1);
        }

        private async Task OnPlayBackFinished(LavalinkGuildConnection conn, TrackFinishEventArgs args)
        {
            if (args.Reason == TrackEndReason.Finished)
            {
                var curr = conn.CurrentState.CurrentTrack;
                _queue.Remove(curr);
                await conn.PlayAsync(_queue.First());
            }
            
            if (args.Reason == TrackEndReason.Stopped)
            {
                _queue.Clear();
            }
        }

        [Command("skip")]
        public async Task SkipTrack(CommandContext ctx)
        {
            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null || userVC == null)
            {
                await ctx.Channel.SendMessageAsync("Please enter a VC!");
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection is not established!");
                return;
            }

            if (userVC.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Please enter a valic VC");
                return;
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(userVC);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to connect to the dedicated server");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null) 
            {
                await ctx.Channel.SendMessageAsync("No Tracks are in charge");
                return;
            }

            
            await conn.PlayAsync(_queue[(_next + 2) % _queue.Count]);

            string musicDescription = $"Now playing: {_queue[_next].Title} \n"
                                      + $"Author: {_queue[_next].Author} \n"
                                      + $"URL: {_queue[_next].Uri} \n"
                                      + $"List size: {_queue.Count}";
            
            var skipEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.SpringGreen,
                Title = "Track is skipped",
                Description = musicDescription
            };

            await ctx.Channel.SendMessageAsync(embed: skipEmbed);
        }

        [Command("clear")]
        public async Task ClearTracks(CommandContext ctx)
        {
            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null || userVC == null)
            {
                await ctx.Channel.SendMessageAsync("Please enter a VC!");
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection is not established!");
                return;
            }

            if (userVC.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Please enter a valic VC");
                return;
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(userVC);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to connect to the dedicated server");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null) 
            {
                await ctx.Channel.SendMessageAsync("No Tracks are in charge");
                return;
            }

            await conn.StopAsync();
            _queue.Clear();
        }

        [Command("pause")]
        public async Task PauseTrack(CommandContext ctx)
        {
            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null || userVC == null)
            {
                await ctx.Channel.SendMessageAsync("Please enter a VC!");
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection is not established!");
                return;
            }

            if (userVC.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Please enter a valic VC");
                return;
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(userVC);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to connect to the dedicated server");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null) 
            {
                await ctx.Channel.SendMessageAsync("No Tracks are in charge");
                return;
            }

            await conn.PauseAsync();

            var pausedEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Yellow,
                Title = "Track is paused"
            };

            await ctx.Channel.SendMessageAsync(embed: pausedEmbed);
        }

        [Command("stop")]
        public async Task StopTrack(CommandContext ctx)
        {
            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null || userVC == null)
            {
                await ctx.Channel.SendMessageAsync("Please enter a VC!");
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection is not established!");
                return;
            }

            if (userVC.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Please enter a valic VC");
                return;
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(userVC);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to connect to the dedicated server");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No Tracks are in charge");
                return;
            }

            await conn.StopAsync();
            await conn.DisconnectAsync();
            _queue.Clear();

            var stoppedEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.DarkRed,
                Title = $"Track is stopped by {ctx.Member.Nickname}"
            };

            await ctx.Channel.SendMessageAsync(embed: stoppedEmbed);
        }

        [Command("resume")]
        public async Task ResumeTrack(CommandContext ctx)
        {
            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            if (ctx.Member.VoiceState == null || userVC == null)
            {
                await ctx.Channel.SendMessageAsync("Please enter a VC!");
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection is not established!");
                return;
            }

            if (userVC.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Please enter a valic VC");
                return;
            }

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(userVC);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to connect to the dedicated server");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No Tracks are in charge");
                return;
            }

            await conn.ResumeAsync();

            var resumeEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Title = "Track is stopped"
            };

            await ctx.Channel.SendMessageAsync(embed: resumeEmbed);
        }
    }
}

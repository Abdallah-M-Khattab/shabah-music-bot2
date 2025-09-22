using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Victoria;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    MessageCacheSize = 100
                }));

                services.AddSingleton(x => new CommandService(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async
                }));

                var lavalink = new LavaNode(new LavaConfig()
                {
                    Hostname = Environment.GetEnvironmentVariable("LAVALINK_HOSTNAME"),
                    Port = ushort.Parse(Environment.GetEnvironmentVariable("LAVALINK_PORT") ?? "2333"),
                    Authorization = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD")
                });
                services.AddSingleton(lavalink);

                services.AddSingleton<CommandHandler>();
                services.AddHostedService<BotService>();
            });

        await builder.RunConsoleAsync();
    }
}

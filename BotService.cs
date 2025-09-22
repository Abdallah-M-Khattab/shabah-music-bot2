using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
using Victoria;
using Discord;

public class BotService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _handler;
    private readonly LavaNode _lava;

    public BotService(IServiceProvider services)
    {
        _services = services;
        _client = services.GetRequiredService<DiscordSocketClient>();
        _handler = services.GetRequiredService<CommandHandler>();
        _lava = services.GetRequiredService<LavaNode>();

        _client.Log += LogAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("Missing DISCORD_TOKEN env variable.");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.Ready += async () =>
        {
            if (!_lava.IsConnected)
            {
                try { await _lava.ConnectAsync(); }
                catch (Exception ex)
                {
                    await LogAsync(new LogMessage(LogSeverity.Error, "LavaNode", "Couldn't connect to Lavalink", ex));
                }
            }
        };

        await _handler.InitializeAsync(_services);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
        if (_lava.IsConnected) await _lava.DisconnectAsync();
    }

    private Task LogAsync(LogMessage msg)
    {
        Console.WriteLine($"[{msg.Severity}] {msg.Source}: {msg.Message} {msg.Exception}");
        return Task.CompletedTask;
    }
}

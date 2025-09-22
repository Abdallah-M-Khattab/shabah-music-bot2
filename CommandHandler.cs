using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private IServiceProvider _services;

    public CommandHandler(IServiceProvider services, DiscordSocketClient client, CommandService commands)
    {
        _services = services;
        _client = client;
        _commands = commands;

        _client.MessageReceived += HandleMessageAsync;
    }

    public async Task InitializeAsync(IServiceProvider services)
    {
        _services = services;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    }

    private async Task HandleMessageAsync(SocketMessage s)
    {
        if (!(s is SocketUserMessage msg)) return;
        if (msg.Author.IsBot) return;

        int argPos = 0;
        var prefix = Environment.GetEnvironmentVariable("PREFIX") ?? "-";
        if (!(msg.HasStringPrefix(prefix, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

        var context = new SocketCommandContext(_client, msg);
        await _commands.ExecuteAsync(context, argPos, _services);
    }
}

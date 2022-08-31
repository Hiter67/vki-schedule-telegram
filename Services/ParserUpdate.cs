using MongoDB.Bson;
using Telegram.Bot;
using Telegram.Bot.Types;
using vki_schedule_telegram.Configurations;
using vki_schedule_telegram.Models;

namespace vki_schedule_telegram.Services;

public class ParserUpdate : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ParserUpdate> _logger;
    private readonly BotConfiguration _botConfiguration;
    private readonly MongoService _mongo;
    private readonly ParserService _parserService;
    private Timer? _timer = null;

    public ParserUpdate(ITelegramBotClient botClient, 
        ILogger<ParserUpdate> logger, 
        IConfiguration configuration, 
        MongoService mongo, 
        ParserService parserService)
    {
        _mongo = mongo;
        _parserService = parserService;
        _botConfiguration = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
        _botClient = botClient;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running");

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(_botConfiguration.UpdateAwait));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    { 
        await CheckUpdate(
            "Расписание обновилось", 
            await _mongo.GetParsed(ParserService.Schedule),
            await _parserService.GetSchedule());
    }

    private async Task CheckUpdate(string message, Parsed old, Parsed naw)
    {
        if (old == null)
            await _mongo.AddParsed(naw);
        else
        {
            if (old.Data.Count != naw.Data.Count)
            {
                await _mongo.UpdateParser(naw);
                await Notify(message);
                return;
            }
            for (int i = 0; i < old.Data.Count; i++)
            {
                if (old.Data[i].Name != naw.Data[i].Name || 
                    old.Data[i].Url != naw.Data[i].Url)
                {
                    await _mongo.UpdateParser(naw);
                    await Notify(message);
                    return;
                }
            }
        }
    }

    private async Task Notify(string message)
    {
        var users = await _mongo.GetUsers(string.Empty, string.Empty);
        
        if (users == null) return;
        
        foreach (var user in users)
        {
            await _botClient.SendTextMessageAsync(user.TgId, message);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
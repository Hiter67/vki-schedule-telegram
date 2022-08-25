using Telegram.Bot;
using Telegram.Bot.Types;
using vki_schedule_telegram.Configurations;

namespace vki_schedule_telegram.Services;

public class ParserUpdate : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ParserUpdate> _logger;
    private readonly BotConfiguration _botConfiguration;
    private Timer? _timer = null;

    public ParserUpdate(ITelegramBotClient botClient, ILogger<ParserUpdate> logger, IConfiguration configuration)
    {
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

    private void DoWork(object? state)
    {
        _botClient.SendTextMessageAsync(new ChatId(478045669),$"Timed Hosted Service is working. UpdateAwait: {_botConfiguration.UpdateAwait}");
        _logger.LogInformation(
            $"Timed Hosted Service is working. UpdateAwait: {_botConfiguration.UpdateAwait}" );
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
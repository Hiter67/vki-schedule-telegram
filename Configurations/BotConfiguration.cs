namespace vki_schedule_telegram.Configurations;

public class BotConfiguration
{
    public string BotToken { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public int UpdateAwait { get; init; }
}

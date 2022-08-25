using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

//using NLog;

namespace vki_schedule_telegram.Services
{
    public class HandleUpdateService
    {
        
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HandleUpdateService> _logger;
        public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }
        private Task HandleErrorAsync(Exception exception)
        {
            _logger.LogWarning(exception.Message);
            return Task.CompletedTask;
        }

        public async Task EchoAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
                /*UpdateType.MyChatMember => BotOnMyChatMemberReceived(botClient, update.MyChatMember!),*/
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }

        private Task BotOnMessageReceived(Message message)
        {
            if (message.Type != MessageType.Text)
                return Task.CompletedTask;
            _ = message.Text!.Split(' ')[0] switch
            {
                _ => Task.CompletedTask
            };
            _logger.LogInformation(message.Text);
            _botClient.SendTextMessageAsync(message.Chat.Id, message.Text);
            return Task.CompletedTask;
        }
        private Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            return Task.CompletedTask;
        }
        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogWarning("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
    }
}
using MongoDB.Bson;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using vki_schedule_telegram.Models;

//using NLog;

namespace vki_schedule_telegram.Services
{
    public class HandleUpdateService
    {
        
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HandleUpdateService> _logger;
        private readonly MongoService _mongo;

        private readonly ReplyKeyboardMarkup defaultKB = new(
            new[]
            {
                new KeyboardButton[] { "Расписание", "Звонки"},
                new KeyboardButton[] { "Списки", "Аттестация" },
            }) { ResizeKeyboard = true };

        public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger, MongoService mongo)
        {
            _mongo = mongo;
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
                UpdateType.MyChatMember => BotOnMyChatMemberReceived(update.MyChatMember!),
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

        private async Task BotOnMyChatMemberReceived(ChatMemberUpdated updateMyChatMember)
        {
            //_logger.LogInformation(updateMyChatMember.ToJson());
            if (updateMyChatMember.NewChatMember.Status == ChatMemberStatus.Member)
            {
                await _mongo.AddUser(new()
                {
                    Name = updateMyChatMember.Chat.Username, 
                    TgId = updateMyChatMember.Chat.Id
                });
            }
            if (updateMyChatMember.NewChatMember.Status == ChatMemberStatus.Kicked)
            {
                await _mongo.RemoveUser(updateMyChatMember.Chat.Id);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            if (message.Type != MessageType.Text)
                return;
            var action = message.Text!.Split(' ')[0].ToLower() switch
            {
                "расписание" => SendParsed(message, await _mongo.GetParsed("schedule")),
                _ => SendKeyboard(message, defaultKB)
            };
            var sentMessage = await action;
            _logger.LogInformation(message.Text);
            //_botClient.SendTextMessageAsync(message.Chat.Id, message.Text);
        }
        
        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            var data = callbackQuery.Data!.Split();
            var parsed = await _mongo.GetParsed(data[0]);
            if (data.Length == 2)
            {
                _ = await SendDocument(callbackQuery.Message!, parsed.Data![Convert.ToInt32(data[1])].Url);
            }
        }
        
        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogWarning("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
        private async Task<Message> SendParsed(Message message, Parsed parsed)
        {
            return await SendInlineKeyboard(message, await DataToIKM(parsed),"Выберете:");
        }

        private Task<InlineKeyboardMarkup> DataToIKM(Parsed parsed)
        {
            List<InlineKeyboardButton[]> bts = new();
            for (int i = 0; i < parsed.Data.Count; i++)
            {
                bts.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        parsed.Data[i].Name, 
                        $"{parsed.Name} {i}" )
                });
            }

            return Task.FromResult(new InlineKeyboardMarkup(bts));
        }

        private async Task<Message> SendKeyboard(Message message, ReplyKeyboardMarkup kb)
        {
            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Выберите:",
                replyMarkup: kb);
        }
        private async Task<Message> SendInlineKeyboard(Message message, InlineKeyboardMarkup kb, string text)
        {
            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: text,
                replyMarkup: kb
            );
        }
        private async Task<Message> SendDocument(Message message, string link) // string name
        {

            return await _botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: new InputOnlineFile(link)
            );
        }
    }
}
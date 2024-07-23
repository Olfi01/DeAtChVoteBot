using Telegram.Bot;
using Telegram.Bot.Types;

namespace DeAtChVoteBot.Services
{
    public class MessageHandler(ITelegramBotClient botClient)
    {
        public async Task RespondToPingMessage(Message message)
        {
            if (message == null) return;
            await botClient.SendTextMessageAsync(message.Chat.Id, "Pong!", replyToMessageId: message.MessageId);
        }
    }
}

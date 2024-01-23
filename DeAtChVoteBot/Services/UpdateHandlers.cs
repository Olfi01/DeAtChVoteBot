using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace DeAtChVoteBot.Services;

public class UpdateHandlers
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandlers> _logger;
    private readonly BotConfiguration _botConfig;

    public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger, IOptions<BotConfiguration> botOptions)
    {
        _botClient = botClient;
        _logger = logger;
        _botConfig = botOptions.Value;
    }

    public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {

    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null) return;
        string data = callbackQuery.Data;
        if (data[..8] != BotConfiguration.InstanceId)
        {
            if (callbackQuery.Message != null)
            {
                await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
            }
            return;
        }
        string[] split = data.Split(':');
        // TODO: handle data with the pattern InstanceId:Category:Vote
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}

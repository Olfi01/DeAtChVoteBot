using DeAtChVoteBot.Database;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DeAtChVoteBot.Services;

public class UpdateHandlers(ILogger<UpdateHandlers> logger, IServiceProvider serviceProvider, BotDataContext dbContext)
{
    public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { Text: { } } message } => BotOnTextMessageReceived(message, cancellationToken),
            _ => Task.CompletedTask
        };

        await handler;
    }

    private async Task BotOnTextMessageReceived(Message message, CancellationToken cancellationToken)
    {
        if (message.From == null || !dbContext.Admins.Any(a => a.TgId == message.From.Id) || cancellationToken.IsCancellationRequested) return;
        var pollService = serviceProvider.GetRequiredService<ManagePolls>();
        string text = message.Text!;
        text = text.Contains('@') ? text.Remove(text.IndexOf('@')) : text;
        var handler = text switch
        {
            "/sendpoll" => pollService.OpenNewPolls(),
            "/closepoll" => pollService.CloseCurrentPolls(),
            _ => Task.CompletedTask
        };
        await handler;
    }
}

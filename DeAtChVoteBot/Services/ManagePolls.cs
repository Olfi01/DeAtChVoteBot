
using DeAtChVoteBot.Database;
using DeAtChVoteBot.Database.Types;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DeAtChVoteBot.Services;

public class ManagePolls(ITelegramBotClient botClient, IOptions<BotConfiguration> botOptions, BotDataContext dbContext)
{
    private readonly BotConfiguration botConfig = botOptions.Value;
    private static readonly CultureInfo culture = new("de-DE");

    public async Task CloseCurrentPolls()
    {
        foreach (var poll in dbContext.Polls.ToList())
        {
            await ClosePoll(poll);
        }
        dbContext.SaveChanges();
    }

    private async Task ClosePoll(DbPoll poll)
    {
        Poll closedPoll = await botClient.StopPollAsync(botConfig.PollChannel, poll.MessageId);

        var potentialWinners = closedPoll.Options.GroupBy(o => o.VoterCount).MaxBy(g => g.Key)!;
        var winner = potentialWinners.ElementAt(Random.Shared.Next(potentialWinners.Count())).Text;

        dbContext.Winners.RemoveRange(dbContext.Winners.Where(winner => winner.Option.Category == poll.Category));
        if (poll.Category.ExcludeLastWinner)
        {
            dbContext.Winners.Add(new Winner { Option = dbContext.Options.First(o => o.Name == winner) });
        }
        dbContext.Polls.Remove(poll);

        await AnnounceResults(winner, poll.MessageId);
    }

    private async Task AnnounceResults(string winner, int messageId)
    {
        var message = await botClient.SendTextMessageAsync(botConfig.PollChannel, $"Gewonnen hat: {winner}", replyToMessageId: messageId);
        await botClient.ForwardMessageAsync(botConfig.AdminChat, botConfig.PollChannel, message.MessageId);
    }

    public async Task OpenNewPolls()
    {
        foreach (var category in dbContext.Categories.ToList())
        {
            await OpenPoll(category);
        }
        dbContext.SaveChanges();
    }

    private async Task OpenPoll(Category category)
    {
        DateTime targetDate = DateTime.Now.AddDays(1);
        string day = culture.DateTimeFormat.GetDayName(targetDate.DayOfWeek);
        string date = targetDate.ToString(culture.DateTimeFormat.ShortDatePattern);
        string pollQuestion = $"Große Runde für {day}, den {date} ({category.Name}):";
        var winners = category.ExcludeLastWinner ? dbContext.Winners.Select(w => w.Option).ToList() : [];
        var pollOptions = category.Options.Except(winners).Select(o => o.Name);
        Message pollMessage = await botClient.SendPollAsync(botConfig.PollChannel, pollQuestion, pollOptions, protectContent: true);
        dbContext.Polls.Add(new DbPoll { Category = category, MessageId = pollMessage.MessageId });
    }
}
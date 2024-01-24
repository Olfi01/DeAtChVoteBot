
using Microsoft.Extensions.Options;

namespace DeAtChVoteBot.Services;

public class ConfigureCronjob(
        ILogger<ConfigureCronjob> logger,
        IServiceProvider serviceProvider,
        IOptions<BotConfiguration> botOptions) : IHostedService
{
    private Timer? timer;
    private readonly BotConfiguration botConfig = botOptions.Value;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        TimeSpan dueTime = TimeOnly.Parse(botConfig.PollTime).ToTimeSpan() - DateTime.Now.TimeOfDay;
        if (dueTime < TimeSpan.Zero) dueTime += TimeSpan.FromDays(1);
        timer = new Timer(TimerElapsed, null, dueTime, TimeSpan.FromHours(24));
        return Task.CompletedTask;
    }

    private async void TimerElapsed(object? state)
    {
        var scope = serviceProvider.CreateScope();
        var pollService = scope.ServiceProvider.GetRequiredService<ManagePolls>();
        logger.LogInformation("Closing polls");
        await pollService.CloseCurrentPolls();
        logger.LogInformation("Opening new polls");
        await pollService.OpenNewPolls();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, Timeout.Infinite);
        return Task.CompletedTask;
    }
}
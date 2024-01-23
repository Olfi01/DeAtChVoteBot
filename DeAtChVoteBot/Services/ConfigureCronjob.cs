
using Microsoft.Extensions.Options;

namespace DeAtChVoteBot.Services;

public class ConfigureCronjob : IHostedService
{
    private Timer? timer;
    private readonly ILogger<ConfigureCronjob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BotConfiguration _botConfig;

    public ConfigureCronjob(
            ILogger<ConfigureCronjob> logger,
            IServiceProvider serviceProvider,
            IOptions<BotConfiguration> botOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _botConfig = botOptions.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        TimeSpan dueTime = TimeOnly.Parse(_botConfig.PollTime).ToTimeSpan() - DateTime.Now.TimeOfDay;
        timer = new Timer(TimerElapsed, null, dueTime, TimeSpan.FromHours(24));
        return Task.CompletedTask;
    }

    private void TimerElapsed(object? state)
    {
        var pollService = _serviceProvider.GetRequiredService<ManagePolls>();
        pollService.CloseCurrentPoll();
        pollService.OpenNewPoll();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, Timeout.Infinite);
        return Task.CompletedTask;
    }
}
namespace DeAtChVoteBot;

using DeAtChVoteBot.Controllers;
using DeAtChVoteBot.Database;
using DeAtChVoteBot.Helpers;
using DeAtChVoteBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Setup Bot configuration
        builder.Configuration.AddEnvironmentVariables("WWVOTE__");
        var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
        builder.Services.Configure<BotConfiguration>(botConfigurationSection);

        var botConfiguration = botConfigurationSection.Get<BotConfiguration>()!;

        builder.Services.AddDbContext<BotDataContext>(options => options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!));

        builder.Services.AddHealthChecks().AddDbContextCheck<BotDataContext>();

        // Register named HttpClient to get benefits of IHttpClientFactory
        // and consume it with ITelegramBotClient typed client.
        // More read:
        //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#typed-clients
        //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        builder.Services.AddHttpClient("telegram_bot_client")
                    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                    {
                        BotConfiguration? botConfig = sp.GetConfiguration<BotConfiguration>();
                        TelegramBotClientOptions options = new(botConfig.BotToken);
                        return new TelegramBotClient(options, httpClient);
                    });

        builder.Services.AddScoped<UpdateHandlers>();
        builder.Services.AddScoped<ManagePolls>();

        // There are several strategies for completing asynchronous tasks during startup.
        // Some of them could be found in this article https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
        // We are going to use IHostedService to add and later remove Webhook
        builder.Services.AddHostedService<ConfigureWebhook>();
        builder.Services.AddHostedService<ConfigureCronjob>();

        // The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
        // incoming webhook updates and send serialized responses back.
        // Read more about adding Newtonsoft.Json to ASP.NET Core pipeline:
        //   https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-6.0#add-newtonsoftjson-based-json-format-support
        builder.Services
            .AddControllers()
            .AddNewtonsoftJson();

        var app = builder.Build();
        CreateDbIfNotExists(app);
        // Construct webhook route from the Route configuration parameter
        // It is expected that BotController has single method accepting Update
        app.MapBotWebhookRoute<BotController>(route: botConfiguration.Route);
        app.MapHealthChecks("/healthz");
        app.MapControllers();
        app.Run();
    }

    private static void CreateDbIfNotExists(WebApplication host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<BotDataContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }
    }
}

public class BotConfiguration
{
    public static Random Random { get; } = new Random();
    public static string InstanceId { get; } = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 8).Select(s => s[Random.Next(s.Length)]).ToArray());
    public static readonly string Configuration = "BotConfiguration";

    public string BotToken { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public string Route { get; init; } = default!;
    public string SecretToken { get; init; } = default!;
    public string PollTime { get; init; } = default!;
    public string PollChannel { get; init; } = default!;
    public string AdminChat { get; init; } = default!;
}

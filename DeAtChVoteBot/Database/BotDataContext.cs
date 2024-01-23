using DeAtChVoteBot.Database.Types;
using Microsoft.EntityFrameworkCore;

namespace DeAtChVoteBot.Database;

public class BotDataContext : DbContext
{
    public BotDataContext(DbContextOptions<BotDataContext> options) : base(options)
    {

    }

    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Option> Options { get; set; } = default!;
}
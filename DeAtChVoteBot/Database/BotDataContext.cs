using DeAtChVoteBot.Database.Types;
using Microsoft.EntityFrameworkCore;

namespace DeAtChVoteBot.Database;

public class BotDataContext : DbContext
{
    public BotDataContext(DbContextOptions<BotDataContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder) => builder.UseLazyLoadingProxies();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Admin>().HasIndex(a => a.TgId).IsUnique();
        builder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        builder.Entity<Option>().HasIndex(o => o.Name).IsUnique();
        builder.Entity<Winner>().HasOne(w => w.Option);
    }

    public DbSet<Admin> Admins { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Option> Options { get; set; } = default!;
    public DbSet<DbPoll> Polls { get; set; } = default!;
    public DbSet<Winner> Winners { get; set; } = default!;
}
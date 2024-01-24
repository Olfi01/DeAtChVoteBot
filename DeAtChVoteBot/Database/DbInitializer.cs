using DeAtChVoteBot.Database.Types;

namespace DeAtChVoteBot.Database
{
    public static class DbInitializer
    {
        public static void Initialize(BotDataContext context)
        {
            context.Database.EnsureCreated();

            if (context.Categories.Any())
            {
                return;
            }

            Category language = new() { Name = "Sprache", Options = [], ExcludeLastWinner = true };
            Category mode = new() { Name = "Modus", Options = [], ExcludeLastWinner = true };
            Category thief = new() { Name = "Dieb", Options = [], ExcludeLastWinner = false };
            var categories = new Category[]
            {
                language,
                mode,
                thief
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var options = new Option[]
            {
                new() { Category = language, Name = "Normal" },
                new() { Category = language, Name = "Amnesia" },
                new() { Category = language, Name = "Pokémon" },
                new() { Category = language, Name = "Schwäbisch" },
                new() { Category = language, Name = "Emoji" },
                new() { Category = language, Name = "Harry Potter" },
                new() { Category = language, Name = "Spezial" },
                new() { Category = mode, Name = "Nichts" },
                new() { Category = mode, Name = "Secret lynch" },
                new() { Category = mode, Name = "Kein Verraten der Rollen nach dem Tod" },
                new() { Category = mode, Name = "Beides" },
                new() { Category = thief, Name = "Dieb nur in der ersten Nacht" },
                new() { Category = thief, Name = "Dieb in jeder Nacht" },
            };
            context.Options.AddRange(options);
            context.SaveChanges();

            var admins = new Admin[]
            {
                new() { TgId = 180562990 },
                new() { TgId = 32659634 },
                new() { TgId = 222697924 },
                new() { TgId = 32248944 },
                new() { TgId = 292959071 },
                new() { TgId = 196490244 },
                new() { TgId = 79312108 },
                new() { TgId = 148343980 },
                new() { TgId = 40890637 },
                new() { TgId = 267376056 },
                new() { TgId = 222891511 },
                new() { TgId = 43817863 },
                new() { TgId = 178145356 }
            };
            context.Admins.AddRange(admins); 
            context.SaveChanges();
        }
    }
}

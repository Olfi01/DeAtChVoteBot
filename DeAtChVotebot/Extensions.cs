using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace DeAtChVotebot
{
    public static class Extensions
    {
        public static List<string> Copy(this List<string> list)
        {
            var l = new List<string>();
            foreach (var i in list) l.Add(i);
            return l;
        }

        public static async Task ClearUpdates(this ITelegramBotClient client)
        {
            var updates = await client.GetUpdatesAsync(-1);
            if (updates.Length > 0) await client.GetUpdatesAsync(updates.First().Id + 1);
        }
    }
}

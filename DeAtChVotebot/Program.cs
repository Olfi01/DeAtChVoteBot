using DeAtChVotebot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace DeAtChVoteBot
{
    public static class Program
    {
        private const string baseDir = "C:\\Olfi01\\DEATCHVote\\";
        private const string wonYesterdayPath = baseDir + "yesterday.txt";
#if DEBUG
        private const string channelName = "@flomstestchannel";
#else
        private const string channelName = "@werwolfinfo";
#endif
        private static readonly List<long> admins = new List<long>()
        { 180562990, 32659634, 222697924, 32248944, 292959071, 196490244, 79312108, 148343980,
            40890637, 267376056, 222891511, 43817863, 178145356 };
        private const long adminChatId = -1001118086649;
        private static readonly List<string> languagesOriginal = new List<string>() { "Amnesia", "Pokémon", "Schwäbisch", "Emoji", "Spezial" };
        private static List<string> languages = languagesOriginal.Copy();
        private static readonly List<string> modesOriginal = new List<string>()
        { "Secret lynch", "Kein Verraten der Rollen nach dem Tod", "Beides" };
        private static List<string> modes = modesOriginal.Copy();
        private static readonly List<string> thieves = new List<string>() { "Dieb jede Nacht", "Dieb nur in der ersten Nacht" };
        private static TelegramBotClient client;
        private static string langMsgText = "";
        private static string modeMsgText = "";
        private static string thiefMsgText = "";
        private static readonly Dictionary<long, string> langVotes = new Dictionary<long, string>();
        private static readonly Dictionary<long, string> modeVotes = new Dictionary<long, string>();
        private static readonly Dictionary<long, string> thiefVotes = new Dictionary<long, string>();
        private static int langMsgId = 0;
        private static int modeMsgId = 0;
        private static int thiefMsgId = 0;
        private static readonly List<Action<CallbackQuery>> callbackQueryHandlers = new List<Action<CallbackQuery>>();
        private static readonly List<Action<Message>> messageHandlers = new List<Action<Message>>();

        public static void Main(string[] args)
        {
            Directory.CreateDirectory(baseDir);
            if (!File.Exists(wonYesterdayPath)) File.Create(wonYesterdayPath);
            client = new TelegramBotClient(args[0]);
            client.ClearUpdates().Wait();
            client.StartReceiving(Client_UpdateHandler, Client_ErrorHandler);
            bool running = true;
            Timer timer = new Timer(CloseAndOpenPoll);
            DateTime now = DateTime.Now;
            DateTime grTime = new DateTime(now.Year, now.Month, now.Hour >= 21 ? now.Day + 1 : now.Day, 21, 0, 0);
            timer.Change(grTime - DateTime.Now, TimeSpan.FromHours(24));
            while (running)
            {
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "stop":
                    case "stopbot":
                        running = false;
                        break;
                    case "start":
                        var t = new Thread(() => client.StartReceiving(Client_UpdateHandler, Client_ErrorHandler));
                        t.Start();
                        break;
                }
            }
        }

        private static async Task Client_ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.ToString());
            await Task.CompletedTask;
        }

        private static async Task Client_UpdateHandler(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                Parallel.ForEach(messageHandlers, (handler) => handler.Invoke(update.Message));
                await Client_OnMessageUpdate(update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                Parallel.ForEach(callbackQueryHandlers, (handler) => handler.Invoke(update.CallbackQuery));
                await Client_OnCallbackQuery(update.CallbackQuery);
            }
        }

        private static async void CloseAndOpenPoll(object state)
        {
            await client.SendTextMessageAsync(adminChatId, "Ergebnisse: \n\n\n" + GetCurrentLangPoll() + "\n\n\n" + GetCurrentModePoll() + "\n\n\n" + GetCurrentThiefPoll());
            await ClosePoll();
            await SendPoll(DateTime.Now.AddDays(1));
        }

        private static async Task Client_OnCallbackQuery(CallbackQuery query)
        {
            var data = query.Data;
            if (data == "today" || data == "tomorrow" || data == "lang" || data == "mode" || data == "none") return;
            if (languages.Contains(data) || data == "Normal")
            {
                if (langVotes.ContainsKey(query.From.Id))
                {
                    if (data == langVotes[query.From.Id])
                    {
                        langVotes.Remove(query.From.Id);
                        await client.AnswerCallbackQueryAsync(query.Id, "Du hast deine Stimme zurückgezogen.");
                        await RefreshLangMsg();
                        return;
                    }
                    langVotes[query.From.Id] = data;
                    await client.AnswerCallbackQueryAsync(query.Id, $"Du hast für {data} abgestimmt.");
                    await RefreshLangMsg();
                    return;
                }
                langVotes.Add(query.From.Id, data);
                await client.AnswerCallbackQueryAsync(query.Id, $"Du hast für {data} abgestimmt.");
                await RefreshLangMsg();
                return;
            }
            if (modes.Contains(data) || data == "Nichts")
            {
                if (modeVotes.ContainsKey(query.From.Id))
                {
                    if (data == modeVotes[query.From.Id])
                    {
                        modeVotes.Remove(query.From.Id);
                        await client.AnswerCallbackQueryAsync(query.Id, "Du hast deine Stimme zurückgezogen.");
                        await RefreshModeMsg();
                        return;
                    }
                    modeVotes[query.From.Id] = data;
                    await client.AnswerCallbackQueryAsync(query.Id, $"Du hast für {data} abgestimmt.");
                    await RefreshModeMsg();
                    return;
                }
                modeVotes.Add(query.From.Id, data);
                await client.AnswerCallbackQueryAsync(query.Id, $"Du hast für {data} abgestimmt.");
                await RefreshModeMsg();
                return;
            }
            if (thiefVotes.ContainsKey(query.From.Id))
            {
                if (data == thiefVotes[query.From.Id])
                {
                    thiefVotes.Remove(query.From.Id);
                    await client.AnswerCallbackQueryAsync(query.Id, "Du hast deine Stimme zurückgezogen.");
                    await RefreshThiefMsg();
                    return;
                }
                thiefVotes[query.From.Id] = data;
                await client.AnswerCallbackQueryAsync(query.Id, $"Du hast für {data} abgestimmt.");
                await RefreshThiefMsg();
                return;
            }
            thiefVotes.Add(query.From.Id, data);
            await client.AnswerCallbackQueryAsync(query.Id, $"Du hast für {data} abgestimmt.");
            await RefreshThiefMsg();
            return;
        }

        private static async Task Client_OnMessageUpdate(Message msg)
        {
            if (msg.Type != MessageType.Text
                || !admins.Contains(msg.From.Id)) return;
            if (msg.Entities == null || msg.Entities.Count() < 1 || msg.Entities[0].Type != MessageEntityType.BotCommand || msg.Entities[0].Offset != 0) return;
            var cmd = msg.EntityValues.First();
            if (cmd.Contains("@")) cmd = cmd.Remove(cmd.IndexOf("@"));
            switch (cmd)
            {
                case "/sendpoll":
                    Thread t = new Thread(async () => await SendPoll(msg));
                    t.Start();
                    break;
                case "/closepoll":
                    await client.SendTextMessageAsync(msg.Chat.Id,
                        "Abstimmung geschlossen. Ergebnisse: \n\n\n" + GetCurrentLangPoll() + "\n\n\n" + GetCurrentModePoll());
                    await ClosePoll();
                    langVotes.Clear();
                    modeVotes.Clear();
                    break;
            }
        }

        private static async Task SendPoll(Message msg)
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            var todayTomorrow = new List<string>() { "today", "tomorrow" };
            bool today = false;
            Action<CallbackQuery> cHandler = (query) =>
            {
                if (!admins.Contains(query.From.Id) || !todayTomorrow.Contains(query.Data)) return;
                if (query.Data == "today") today = true;
                mre.Set();
            };
            var sent = await client.SendTextMessageAsync(msg.Chat.Id, "Die Abstimmung für heute oder morgen?",
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("Heute") { CallbackData = "today" },
                    new InlineKeyboardButton("Morgen") { CallbackData = "tomorrow" }
                }));
            try
            {
                callbackQueryHandlers.Add(cHandler);
                mre.WaitOne();
            }
            finally
            {
                callbackQueryHandlers.Remove(cHandler);
            }
            mre.Reset();
            string answer = "none";
            List<string> custom = new List<string>() { "lang", "mode", "none" };
            cHandler = (query) =>
            {
                answer = query.Data;
                if (!admins.Contains(query.From.Id) || !custom.Contains(answer)) return;
                mre.Set();
            };
            await client.EditMessageTextAsync(sent.Chat.Id, sent.MessageId, "Zusätzliche Option?",
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("Sprache") { CallbackData = "lang" },
                    new InlineKeyboardButton("Modus") { CallbackData = "mode" },
                    new InlineKeyboardButton("Keine") { CallbackData = "none" }
                }));
            try
            {
                callbackQueryHandlers.Add(cHandler);
                mre.WaitOne();
            }
            finally
            {
                callbackQueryHandlers.Remove(cHandler);
            }
            if (answer != "none")
            {
                mre.Reset();
                var replyTo = await client.SendTextMessageAsync(msg.Chat.Id, "Antworte auf diese Nachricht bitte mit der zusätzlichen Option." +
                    " Beim Senden von /closepoll wird sie wieder entfernt.", replyMarkup: new ForceReplyMarkup());
                Action<Message> mHandler = (m) =>
                {
                    if (!admins.Contains(m.From.Id) || m.ReplyToMessage == null
                    || m.ReplyToMessage.MessageId != replyTo.MessageId) return;
                    switch (answer)
                    {
                        case "lang":
                            languages.Add(m.Text);
                            break;
                        case "mode":
                            modes.Add(m.Text);
                            break;
                    }
                    mre.Set();
                };
                try
                {
                    messageHandlers.Add(mHandler);
                    mre.WaitOne();
                }
                finally
                {
                    messageHandlers.Remove(mHandler);
                }
            }
            await SendPoll(today ? DateTime.Today : DateTime.Today.AddDays(1));
            await client.EditMessageTextAsync(sent.Chat.Id, sent.MessageId, "Abstimmung wurde gesendet.");
        }

        private static async Task RefreshLangMsg()
        {
            await client.EditMessageTextAsync(channelName, langMsgId, langMsgText + "\n" + GetCurrentLangPoll(), 
                replyMarkup: GetLangReplyMarkup(), parseMode: ParseMode.Markdown);
        }

        private static async Task RefreshModeMsg()
        {
            await client.EditMessageTextAsync(channelName, modeMsgId, modeMsgText + "\n" + GetCurrentModePoll(), 
                replyMarkup: GetModeReplyMarkup(), parseMode: ParseMode.Markdown);
        }

        private static async Task RefreshThiefMsg()
        {
            await client.EditMessageTextAsync(channelName, thiefMsgId, thiefMsgText + "\n" + GetCurrentThiefPoll(),
                replyMarkup: GetThiefReplyMarkup(), parseMode: ParseMode.Markdown);
        }

        private static async Task DeletePoll()
        {
            await client.DeleteMessageAsync(channelName, langMsgId);
            await client.DeleteMessageAsync(channelName, modeMsgId);
        }

        private static async Task ClosePoll()
        {
            await client.EditMessageReplyMarkupAsync(channelName, langMsgId);
            await client.EditMessageReplyMarkupAsync(channelName, modeMsgId);
            await client.EditMessageReplyMarkupAsync(channelName, thiefMsgId);
            var won = languages.OrderBy(x => -langVotes.Count(y => y.Value == x)).First();
            var wonMode = modes.OrderBy(x => -modeVotes.Count(y => y.Value == x)).First();
            File.WriteAllText(wonYesterdayPath, won + "\n" + wonMode);
            languages = languagesOriginal.Copy();
            modes = modesOriginal.Copy();
        }

        private static async Task SendPoll(DateTime targetDate)
        {
            //DeletePoll();
            var culture = new CultureInfo("de-DE");
            var day = culture.DateTimeFormat.GetDayName(targetDate.DayOfWeek);
            langMsgText = $"*Große Runde für {day}, den {targetDate.ToShortDateString()} (Sprache):*";
            Message m = await client.SendTextMessageAsync(channelName, langMsgText + "\n" + GetCurrentLangPoll(),
                replyMarkup: GetLangReplyMarkup(), parseMode: ParseMode.Markdown);
            langMsgId = m.MessageId;
            modeMsgText = $"*Große Runde für {day}, den {targetDate.ToShortDateString()} (Modus):*";
            m = await client.SendTextMessageAsync(channelName, modeMsgText + "\n" + GetCurrentModePoll(), 
                replyMarkup: GetModeReplyMarkup(), parseMode: ParseMode.Markdown);
            modeMsgId = m.MessageId;
            thiefMsgText = $"*Große Runde für {day}, den {targetDate.ToShortDateString()} (Dieb):*";
            m = await client.SendTextMessageAsync(channelName, thiefMsgText + "\n" + GetCurrentThiefPoll(),
                replyMarkup: GetThiefReplyMarkup(), parseMode: ParseMode.Markdown);
            thiefMsgId = m.MessageId;
        }

        private static InlineKeyboardMarkup GetModeReplyMarkup()
        {
            string[] lines = File.ReadAllLines(wonYesterdayPath);
            string yesterday;
            if (lines.Length < 2) yesterday = "";
            else yesterday = lines[1];
            List<string> modesToday = new List<string>() { "Nichts" };
            foreach (var m in modes) if (m != yesterday) modesToday.Add(m);
            var rows = new List<InlineKeyboardButton[]>();
            foreach (var mode in modesToday)
            {
                rows.Add(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton($"{mode} - {modeVotes.Count(x => x.Value == mode)}") { CallbackData = mode }
                });
            }
            return new InlineKeyboardMarkup(rows.ToArray());
        }

        private static string GetCurrentModePoll()
        {
            string[] lines = File.ReadAllLines(wonYesterdayPath);
            string yesterday;
            if (lines.Length < 2) yesterday = "";
            else yesterday = lines[1];
            List<string> modesToday = new List<string>() { "Nichts" };
            foreach (var m in modes) if (m != yesterday) modesToday.Add(m);
            return string.Join("\n\n", modesToday.OrderBy(x => -modeVotes.Count(y => y.Value == x)).Select(x =>
            {
                var c = modeVotes.Count(y => y.Value == x);
                var t = modeVotes.Count;
                float perc = 0;
                if (t != 0)
                    perc = (float)c / t;
                perc = perc * 100;
                var s = $"{x} - {c}\n";
                for (int i = 0; i < perc / 10; i++)
                {
                    s += "👍";
                }
                s += $" {perc}%";
                return s;
            }));
        }

        private static InlineKeyboardMarkup GetLangReplyMarkup()
        {
            var yesterday = File.ReadAllLines(wonYesterdayPath)[0];
            var langsToday = new List<string>() { "Normal" };
            foreach (var l in languages) if (l != yesterday) langsToday.Add(l);
            var rows = new List<InlineKeyboardButton[]>();
            foreach (var lang in langsToday)
            {
                rows.Add(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton($"{lang} - {langVotes.Count(x => x.Value == lang)}") { CallbackData = lang }
                });
            }
            return new InlineKeyboardMarkup(rows.ToArray());
        }

        private static string GetCurrentLangPoll()
        {
            string[] lines = File.ReadAllLines(wonYesterdayPath);
            string yesterday;
            if (lines.Length < 1) yesterday = "";
            else yesterday = lines[0];
            var langsToday = new List<string>() { "Normal" };
            foreach (var l in languages) if (l != yesterday) langsToday.Add(l);
            return string.Join("\n\n", langsToday.OrderBy(x => -langVotes.Count(y => y.Value == x)).Select(x =>
            {
                var c = langVotes.Count(y => y.Value == x);
                var t = langVotes.Count;
                float perc = 0;
                if (t != 0)
                    perc = (float)c / t;
                perc = perc * 100;
                var s = $"{x} - {c}\n";
                for (int i = 0; i < perc / 10; i++)
                {
                    s += "👍";
                }
                s += $" {perc}%";
                return s;
            }));
        }

        private static InlineKeyboardMarkup GetThiefReplyMarkup()
        {
            List<string> thievesToday = new List<string>();
            foreach (var m in thieves) thievesToday.Add(m);
            var rows = new List<InlineKeyboardButton[]>();
            foreach (var thief in thievesToday)
            {
                rows.Add(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton($"{thief} - {thiefVotes.Count(x => x.Value == thief)}") { CallbackData = thief }
                });
            }
            return new InlineKeyboardMarkup(rows.ToArray());
        }

        private static string GetCurrentThiefPoll()
        {
            List<string> thievesToday = new List<string>();
            foreach (var m in thieves) thievesToday.Add(m);
            return string.Join("\n\n", thievesToday.OrderBy(x => -thiefVotes.Count(y => y.Value == x)).Select(x =>
            {
                var c = thiefVotes.Count(y => y.Value == x);
                var t = thiefVotes.Count;
                float perc = 0;
                if (t != 0)
                    perc = (float)c / t;
                perc = perc * 100;
                var s = $"{x} - {c}\n";
                for (int i = 0; i < perc / 10; i++)
                {
                    s += "👍";
                }
                s += $" {perc}%";
                return s;
            }));
        }
    }
}
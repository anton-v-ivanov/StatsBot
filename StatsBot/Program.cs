using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StatsBot
{
    class Program
    {
        private static TelegramBotClient _bot;
        private static readonly Dictionary<long, Dictionary<int, SenderInfo>> StatsDictionary = new Dictionary<long, Dictionary<int, SenderInfo>>();
        private static Timer _timer;
        private static readonly object SyncObj = new object();
        private const string RulesUrl = "";
		const string Rules = "Правила: " + RulesUrl;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            _bot = new TelegramBotClient("");
            _bot.OnMessage += BotOnMessageReceived;
            //Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            //Bot.OnMessageEdited += BotOnMessageReceived;
            //Bot.OnInlineQuery += BotOnInlineQueryReceived;
            //Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            _bot.OnReceiveError += BotOnReceiveError;
            Logger.Info("Bot started");
            _bot.StartReceiving();

            AssemblyLoadContext.Default.Unloading += MethodInvokedOnSigTerm;

            _timer = new Timer(TimeToEraseHistory, null, 0, 1000);
            
            Console.ReadLine();
        }

        private static void MethodInvokedOnSigTerm(AssemblyLoadContext obj)
        {
            Logger.Info("Bot stopped");

            _bot.StopReceiving();
        }

        private static void TimeToEraseHistory(object state)
        {
            var now = DateTime.UtcNow;
            if (now.Hour != 21 || now.Minute != 0 || now.Second != 0)
                return;

            lock (SyncObj)
            {
                SendDailyStats();

                foreach (var senderInfo in StatsDictionary.SelectMany(kv => kv.Value))
                {
                    senderInfo.Value.Counter = 0;
                    senderInfo.Value.MessageTypes.Clear();
                }
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Logger.Error(receiveErrorEventArgs.ApiRequestException);
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            
            if (message == null)
                return;
            
            if (message.Type == MessageType.TextMessage)
            { 
                if (message.Text.StartsWith("/start"))
                {
                    var usage = @"Бот собирает все сообщения в чатике и выводит статистику. Команды:
/stat - статистика
/rules - правила";
                    BotReply(message.Chat.Id, usage);
                }
                else if (message.Text.StartsWith("/stat"))
                {
                    SendStats(message.Chat.Id);
                }
                else if (message.Text.StartsWith("/rules"))
                {
                    BotReply(message.Chat.Id, Rules);
                }
                else
                {
                    AddToStats(message);
                }
            }
            else
            {
                AddToStats(message);                
            }
        }

        private static void BotReply(long chatId, string text)
        {
            try
            {
                _bot.SendTextMessageAsync(chatId, text).Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Logger.Error(e);
                    return true;
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception);    
            }
        }

        private static void SendDailyStats()
        {
            foreach (var kv in StatsDictionary)
            {
                var chatId = kv.Key;
                var dict = kv.Value;
                var statsString = GetStatsString(dict);
                BotReply(chatId, statsString);
            }
        }

        private static void SendStats(long chatId)
        {
            string statsString;
            lock (SyncObj)
            {
                Dictionary<int, SenderInfo> dict;
                if (!StatsDictionary.TryGetValue(chatId, out dict))
                {
                    BotReply(chatId, "Хз, что за чатик");
                    return;
                }

                statsString = GetStatsString(dict);
            }

            BotReply(chatId, statsString);
        }

        private static string GetStatsString(Dictionary<int, SenderInfo> dict)
        {
            var sb = new StringBuilder();
            var result = dict.OrderByDescending(s => s.Value.Counter);

            if (result.Any())
            {
                sb.AppendLine("Топ тунеядцев на сегодня");
                var countPositive = 0;
                int textCount = 0, photoCount = 0, stickerCount = 0, videoCount = 0, audioCount = 0, voiceMessage = 0, otherCount = 0;
                foreach (var kv in result.Where(kv => kv.Value.Counter > 0))
                {
                    countPositive++;
                    sb.AppendLine(string.Format("{0}: {1}", kv.Value.GetName(), kv.Value.Counter));
                    foreach (var messageType in kv.Value.MessageTypes)
                    {
                        switch (messageType.Key)
                        {
                            case MessageType.TextMessage:
                                textCount += messageType.Value;
                                break;
                            case MessageType.PhotoMessage:
                                photoCount += messageType.Value;
                                break;
                            case MessageType.AudioMessage:
                                audioCount += messageType.Value;
                                break;
                            case MessageType.VideoMessage:
                                videoCount += messageType.Value;
                                break;
                            case MessageType.VoiceMessage:
                                voiceMessage += messageType.Value;
                                break;
                            case MessageType.StickerMessage:
                                stickerCount += messageType.Value;
                                break;
                            case MessageType.ServiceMessage:
                                break;
                            default:
                                otherCount += messageType.Value;
                                break;
                        }

                    }
                }

                if (countPositive > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"Текстовых сообщений: {textCount}");
                    sb.AppendLine($"Фоток: {photoCount}");
                    sb.AppendLine($"Стикеров: {stickerCount}");
                    sb.AppendLine($"Видео: {videoCount}");
                    sb.AppendLine($"Аудио: {audioCount}");
                    sb.AppendLine($"Голосовых сообщений: {voiceMessage}");
                    sb.AppendLine($"Других: {otherCount}");
                }
                else
                {
                    sb.Clear();
                    sb.AppendLine("Сегодня все молчали. Либо ещё рано, либо сегодня тусовка.");
                }
            }
            else
            {
                sb.AppendLine("Сегодня все молчали");
            }
            return sb.ToString();
        }

        private static void AddToStats(Message message)
        {
            lock (SyncObj)
            {
                if (!StatsDictionary.ContainsKey(message.Chat.Id))
                {
                    var dict = new Dictionary<int, SenderInfo>
                    {
                        {
                            message.From.Id, new SenderInfo(message.From.Username, message.From.FirstName, message.From.LastName, message.Type)
                        }
                    };
                    StatsDictionary.Add(message.Chat.Id, dict);
                }
                else
                {
                    var dict = StatsDictionary[message.Chat.Id];
                    if (!dict.ContainsKey(message.From.Id))
                    {
                        dict.Add(message.From.Id,
                            new SenderInfo(message.From.Username, message.From.FirstName, message.From.LastName,
                                message.Type));
                    }
                    else
                    {
                        dict[message.From.Id].Counter++;
                        if(dict[message.From.Id].MessageTypes.ContainsKey(message.Type))
                            dict[message.From.Id].MessageTypes[message.Type]++;
                        else
                            dict[message.From.Id].MessageTypes[message.Type] = 1;
                    }
                }

            }
        }
    }

    internal class SenderInfo
    {
        public int Counter { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public readonly Dictionary<MessageType, int> MessageTypes;

        public SenderInfo(string userName, string firstName, string lastName, MessageType type)
        {
            Counter = 1;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            MessageTypes = new Dictionary<MessageType, int> {{type, 1}};
        }

        public string GetName()
        {
            if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                return UserName;

            var result = string.Empty;

            if (!string.IsNullOrEmpty(FirstName))
                result += FirstName;

            if (!string.IsNullOrEmpty(LastName))
                result += " " + LastName;

            return result;
        }
    }
}

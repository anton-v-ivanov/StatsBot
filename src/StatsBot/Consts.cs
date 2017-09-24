using System;

namespace TlenBot
{
	internal static class Consts
	{
        private const string RulesUrl = "https://docs.google.com/document/d/153LbaX847l9-fpiPznRitO-mZSk0Fy2CumZKb8Lt-v0/edit?usp=sharing";
		internal const string Rules = "Правила тлена: " + RulesUrl;
		internal const string Usage = @"Бот собирает все сообщения в чатике и выводит статистику.
Команды:
/rules -- правила
/stats или /stats сегодня -- статистика за сегодня
/stats вчера -- статистика за вчера
/stats неделя -- статистика за последние 7 дней
/stats месяц -- статистика за последние 30 дней
/stats 11.02.17 12.03.17 @tonageme -- статистика с 11.02.17 по 12.03.17 для пользователя @tonageme. Участника можно не указывать, тогда будет для всех. Можно указать либо @UserName, либо Id";

        public static bool IsDev
        {
            get
            {
                var env = Environment.GetEnvironmentVariable("Dev");
                return env != null && env.Equals("true");
            }
        }
    }
}